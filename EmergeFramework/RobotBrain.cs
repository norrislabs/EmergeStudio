using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reflection;
using System.IO;
using EmergeFramework.Communicators;

namespace EmergeFramework
{
    public class RobotBrain
    {
        private RobotSpecification m_specRobot;
        private Dictionary<string, CommLink> m_Channels;
        private string m_TelemetryChannel;
        private string m_RemCtlChannel;

        public enum RunState
        {
            Running,
            Stopped,
            Paused
        }
        private RunState m_RunState = RunState.Stopped;

        private Task m_SensorTask;
        private Task m_BehaviorTask;
        private CancellationTokenSource m_tsCancel;
        private bool m_RunBehaviors = false;
        private bool m_RunSensors = false;

        private long m_SensorRepID = 0;
        private BlockingCollection<SensorRepository> m_SensorReadings;

        private List<Behavior> m_Behaviors;
        private Queue<RequestQueue> m_RequestQueueQueue;
        private RequestQueue m_WinningRequests;
        private string m_LastWinnerName = "";
        
        private ActivityLog m_LogEntries = new ActivityLog();

        public RobotBrain(RobotSpecification specRobot)
        {
            m_specRobot = specRobot;

            // Create comm link channels
            m_Channels = new Dictionary<string, CommLink>();
            foreach (SpecComm cs in specRobot.CommSpecs)
            {
                CommLink comm = null;
                switch (cs.CommunicatorType)
                {
                    case SpecComm.CommType.Serial:
                        comm = new SerialComm();
                        break;

                    case SpecComm.CommType.TCP:
                        comm = new TcpComm();
                        break;
                }
                comm.Initialize(cs.Name, specRobot.RobotID, cs.Parameters);
                m_Channels.Add(cs.Name, comm);

                // Only support for one telemetry channel
                if (cs.HasTelemetry)
                    m_TelemetryChannel = cs.Name;

                // Only support for one remote control channel
                if (cs.HasRemoteControl)
                    m_RemCtlChannel = cs.Name;
            }

            // Load behavior stack
            LoadBehaviorSet();
        }

        public void Start()
        {
            Start(false);
        }

        public void Start(bool StartPaused)
        {
            // Create a new log
            m_LogEntries.Clear();

            // Create sensor repository queue
            ConcurrentQueue<SensorRepository> queue = new ConcurrentQueue<SensorRepository>();
            m_SensorReadings = new BlockingCollection<SensorRepository>(queue, 1);

            // Create RequestQueue queue
            m_RequestQueueQueue = new Queue<RequestQueue>();

            // Start comm links
            Dictionary<string, CommLink>.ValueCollection valueColl = m_Channels.Values;
            foreach (CommLink comm in valueColl)
                comm.Start();

            // Create sensor (producer) task
            m_tsCancel = new CancellationTokenSource();
            m_SensorTask = new Task(() => { ReceiveSensorDataTask(m_specRobot); }, m_tsCancel.Token);

            // Reset all behaviors
            foreach (Behavior behavior in m_Behaviors)
                behavior.Initialize(m_specRobot, m_LogEntries);

            // Create Behavior/Arbitrate (consumer) task
            m_BehaviorTask = new Task(() => { ProcessBehaviorsTask(m_specRobot); }, m_tsCancel.Token);

            if (StartPaused)
            {
                m_RunBehaviors = false;
                m_RunSensors = false;
                m_RunState = RunState.Paused;
            }
            else
            {
                m_RunBehaviors = true;
                m_RunSensors = true;
                m_RunState = RunState.Running;
            }

            // Start tasks
            m_SensorTask.Start();
            m_BehaviorTask.Start();
        }

        public void Stop()
        {
            if (m_RunState != RunState.Stopped)
            {
                // Send halt requests
                foreach (Request request in m_specRobot.HaltRequests)
                {
                    if (m_Channels.ContainsKey(request.Channel))
                    {
                        m_Channels[request.Channel].Send(request.Command);
                        m_LogEntries.AddEntry(new ActivityLogEntry("Sent " + request.Name + " (" + request.Command + ") via the " + request.Channel + " channel from System"));
                    }
                }

                // Cancel the sensor and behavior processing tasks
                m_tsCancel.Cancel();

                // Stop all the comm links
                Dictionary<string, CommLink>.ValueCollection valueColl = m_Channels.Values;
                foreach (CommLink comm in valueColl)
                    comm.Stop();

                m_RunState = RunState.Stopped;
            }
        }

        public void Pause()
        {
            if (m_RunState == RunState.Running)
            {
                m_RunBehaviors = false;
                m_RunSensors = false;
                m_RunState = RunState.Paused;
            }
        }

        public void Resume()
        {
            if (m_RunState == RunState.Stopped)
                Start();
            else
            {
                m_RunBehaviors = true;
                m_RunSensors = true;
                m_RunState = RunState.Running;
            }
        }

        public RunState Status
        {
            get { return m_RunState; }
        }

        public ActivityLog ActivityLog
        {
            get { return m_LogEntries; }
        }

        public CommLink RemoteControlChannel
        {
            get { return m_Channels[m_RemCtlChannel]; }
        }

        public CommLink TelemetryChannel
        {
            get { return m_Channels[m_TelemetryChannel]; }
        }

        public string LastBehaviorWinnerName
        {
            get
            {
                return m_LastWinnerName;
            }
        }

        private void LoadBehaviorSet()
        {
            m_Behaviors = new List<Behavior>();
            List<Behavior> behaviors = new List<Behavior>();

            DirectoryInfo bdi = new DirectoryInfo(m_specRobot.BehaviorsPath);
            foreach(FileInfo fi in bdi.GetFiles("*.dll"))
            {
                if(fi.Name.ToLower().StartsWith("behaviorset"))
                {
                    Assembly loadedAssembly = Assembly.LoadFrom(fi.FullName);
                    foreach (Type type in loadedAssembly.GetTypes())
                    {
                        if (type.BaseType == typeof(Behavior))
                        {
                            if (m_specRobot.BehaviorSpecs.ContainsKey(type.Name))
                            {
                                object instance = loadedAssembly.CreateInstance(type.FullName);
                                Behavior behavior = instance as Behavior;
                                behavior.Initialize(m_specRobot, m_LogEntries);
                                behaviors.Add(behavior);
                            }
                        }
                    }
                }
            }

            // Sort behaviors by priority
            m_Behaviors = behaviors.OrderBy(x => x.Priority).ToList<Behavior>();

            foreach (Behavior behavior in m_Behaviors)
                m_LogEntries.AddEntry(new ActivityLogEntry("Loaded behavior: " + behavior.Name + ", Priority " + behavior.Priority.ToString()));
        }

        private void ReceiveSensorDataTask(RobotSpecification specRobot)
        {
            while (true)
            {
                if (m_tsCancel.IsCancellationRequested)
                {
                    m_LogEntries.AddEntry(new ActivityLogEntry("Sensor Task stopped"));
                    break;
                }
                else
                {
                    if (m_RunSensors)
                    {
                        try
                        {
                            // Time telemetry request/reception
                            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                            // Request and receive remote sensor data via telemetry channel
                            SensorRepository sr = new SensorRepository(specRobot);
                            m_Channels[m_TelemetryChannel].Send("T1");
                            string data = m_Channels[m_TelemetryChannel].ReadLine();

                            sw.Stop();

                            // Parse received sensor data
                            sr.ParseSensorData(data, specRobot.SensorSpecs.Count);
                            if (sr.IsValid)
                            {
                                // Add sensor repository to behavior/arbitrate queue
                                if (m_SensorReadings.TryAdd(sr, 1, m_tsCancel.Token))
                                {
                                    sr.AcquisitionTime = sw.ElapsedMilliseconds;
                                    sr.ID = m_SensorRepID;
                                    m_SensorRepID++;
                                }
                                else
                                {
                                    // Behaviors have not consumed previous sensor data so remove it and get newer
                                    m_LogEntries.AddEntry(new ActivityLogEntry("Sensor data not used."));
                                    if (m_SensorReadings.TryTake(out sr, 1, m_tsCancel.Token))
                                        m_LogEntries.AddEntry(new ActivityLogEntry("Old sensor data removed!"));
                                }
                            }
                            else
                            {
                                // Lost communications with the robot
                                m_LogEntries.AddEntry(new ActivityLogEntry(ActivityLogEntry.LogEntryType.Error, "No communications with the robot!", null));
                                m_LogEntries.AddEntry(new ActivityLogEntry("RX Data = " + data));
                            }

                            m_LogEntries.AddEntry(new ActivityLogEntry("Sensor Acquisition Time", sw.ElapsedMilliseconds));
                        }
                        catch (Exception ex)
                        {
                            m_LogEntries.AddEntry(new ActivityLogEntry("Exception in Sensor Task: " + ex.Message));
                        }
                    }
                }
            }
        }

        private void ProcessBehaviorsTask(RobotSpecification specRobot)
        {
            while (true)
            {
                if (m_tsCancel.IsCancellationRequested)
                {
                    m_LogEntries.AddEntry(new ActivityLogEntry("Behavior Task stopped"));
                    break;
                }
                else
                {
                    try
                    {
                        SensorRepository sr;
                        if (m_SensorReadings.TryTake(out sr, -1, m_tsCancel.Token))
                        {
                            m_LogEntries.AddEntry(new ActivityLogEntry(ActivityLogEntry.LogEntryType.SensorData, "Get sensor data", sr));

                            // Run each Behavior in order of priority
                            if (m_RunBehaviors)
                            {
                                foreach (Behavior behavior in m_Behaviors)
                                {
                                    // Execute the behavior and enqueue any requests it may have generated
                                    RequestQueue requests = behavior.Execute(sr, m_WinningRequests);
                                    if (requests != null && requests.Count > 0)
                                        m_RequestQueueQueue.Enqueue(requests);
                                }
                            }

                            // Arbitrate and send winning behavior's requests
                            if (m_RequestQueueQueue.Count > 0)
                            {
                                // Since behaviors are executed in priority order the highest will always be the first in the request queue queue
                                m_WinningRequests = m_RequestQueueQueue.Dequeue();
                                m_LastWinnerName = m_WinningRequests.BehaviorName;

                                // Send each request out the selected comm link
                                SendRequests(m_WinningRequests);

                                // Clear out the queue for the next time
                                m_RequestQueueQueue.Clear();
                            }
                            else
                                m_WinningRequests = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        m_LogEntries.AddEntry(new ActivityLogEntry("Exception in Behavior Task: " + ex.Message));
                    }
                }
            }
        }

        private void SendRequests(RequestQueue requests)
        {
            // Send requests out each channel
            Dictionary<string, CommLink>.ValueCollection valueColl = m_Channels.Values;
            foreach (CommLink comm in valueColl)
            {
                RequestQueue requestsChannel = new RequestQueue(requests.BehaviorName);
                foreach (Request req in requests)
                {
                    if (req.Channel == comm.Name)
                    {
                        requestsChannel.Enqueue(req);
                        m_LogEntries.AddEntry(new ActivityLogEntry(requestsChannel.BehaviorName + " sent " + req.Name + " (" + req.Command + ") via the " + req.Channel + " channel"));
                    }
                }

                if(requestsChannel.Count > 0)
                    comm.Send(requestsChannel);
            }
        }
    }
}
