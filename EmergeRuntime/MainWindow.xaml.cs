using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using Microsoft.Win32;
using EmergeFramework;

namespace EmergeRuntime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer m_timerUI;
        private RobotBrain m_Robot;
        private RobotSpecification m_specRobot;
        private Dictionary<string, TextBlock> m_Sensors = new Dictionary<string, TextBlock>();
        private Dictionary<string, TextBlock> m_Behaviors = new Dictionary<string, TextBlock>();
        private static LineGraphPlotter m_Plotter;
        private double m_Timeline = 0;
        private RemCtlWnd m_RemCtlWnd;
        private string m_Title;
        private int m_PlotRefreshCount = 0;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Rect bounds = Properties.Settings.Default.WinPosition;
                this.Top = bounds.Top;
                this.Left = bounds.Left;
                this.Height = bounds.Height;
                this.Width = bounds.Width;
            }
            catch
            {
            }

            grdData.ColumnDefinitions[0].Width = new GridLength(Properties.Settings.Default.Splitter1Width);
            grdStats.RowDefinitions[0].Height = new GridLength(Properties.Settings.Default.Splitter3Height);
            grdStats2.ColumnDefinitions[0].Width = new GridLength(Properties.Settings.Default.Splitter2Width);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_Title = this.Title;

            m_Plotter = new LineGraphPlotter(plotter);

            cbMessages.IsChecked = Properties.Settings.Default.ShowMsgs;
            cbErrors.IsChecked = Properties.Settings.Default.ShowErrors;
            cbSensorData.IsChecked = Properties.Settings.Default.ShowSensorData;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(m_Robot != null)
                m_Robot.Stop();

            if(m_timerUI != null)
                m_timerUI.Stop();

            Properties.Settings.Default.Splitter1Width = grdData.ColumnDefinitions[0].Width.Value;
            Properties.Settings.Default.Splitter3Height = grdStats.RowDefinitions[0].Height.Value;
            Properties.Settings.Default.Splitter2Width = grdStats2.ColumnDefinitions[0].Width.Value;
            Properties.Settings.Default.WinPosition = this.RestoreBounds;
            Properties.Settings.Default.ShowMsgs = (bool)cbMessages.IsChecked;
            Properties.Settings.Default.ShowErrors = (bool)cbErrors.IsChecked;
            Properties.Settings.Default.ShowSensorData = (bool)cbSensorData.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void miOpen_Click(object sender, RoutedEventArgs e)
        {
            string Filename = Properties.Settings.Default.LastSpecFile;
            OpenFileDialog dlg = new OpenFileDialog();
            try
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(Filename);
                dlg.FileName = System.IO.Path.GetFileName(Filename);
            }
            catch
            {
                dlg.InitialDirectory = Environment.CurrentDirectory;
                dlg.FileName = "";
            }

            dlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == true)
            {
                btnStop_Click(sender, e);

                Properties.Settings.Default.LastSpecFile = dlg.FileName;

                // Create the robot brain!
                m_specRobot = RobotSpecification.Load(dlg.FileName);
                m_Robot = new RobotBrain(m_specRobot);

                // Setup UI timer
                if (m_timerUI == null)
                {
                    m_timerUI = new DispatcherTimer();
                    m_timerUI.Interval = TimeSpan.FromMilliseconds(1);
                    m_timerUI.Tick += new EventHandler(UpdateUI);
                    m_timerUI.Start();
                }

                this.Title = m_Title + " - " + System.IO.Path.GetFileName(dlg.FileName);
            }
        }

        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            btnClose_Click(sender, e);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (m_Robot.Status == RobotBrain.RunState.Stopped)
            {
                DisplaySensors();
                DisplayBehaviors();
                SetupPlotter();

                m_Robot.Start();
            }
            else if (m_Robot.Status == RobotBrain.RunState.Paused)
            {
                btnPause_Click(sender, e);
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (m_Robot.Status == RobotBrain.RunState.Running)
            {
                m_Robot.Pause();
            }
            else if (m_Robot.Status == RobotBrain.RunState.Paused)
            {
                m_Robot.Resume();
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (m_Robot != null && m_Robot.Status != RobotBrain.RunState.Stopped)
            {
                m_Robot.Stop();
                if (m_RemCtlWnd != null && m_RemCtlWnd.IsVisible)
                    m_RemCtlWnd.Close();
            }
        }

        private void btnRemCtl_Click(object sender, RoutedEventArgs e)
        {
            if (m_RemCtlWnd != null && m_RemCtlWnd.IsVisible)
                return;

            if (m_Robot.Status == RobotBrain.RunState.Stopped)
                m_Robot.Start(true);
            else if (m_Robot.Status == RobotBrain.RunState.Running)
                m_Robot.Pause();

            // Use the last saved location
            Rect loc = Properties.Settings.Default.RemCtlWndPos;

            m_RemCtlWnd = new RemCtlWnd(m_specRobot, m_Robot.RemoteControlChannel);
            m_RemCtlWnd.WindowStartupLocation = WindowStartupLocation.Manual;
            m_RemCtlWnd.Top = loc.Top;
            m_RemCtlWnd.Left = loc.Left;
            m_RemCtlWnd.Width = loc.Width;
            m_RemCtlWnd.Height = loc.Height;

            // Show remote control window
            m_RemCtlWnd.Owner = this;
            m_RemCtlWnd.Closing += new System.ComponentModel.CancelEventHandler(BulkWinClosing);
            m_RemCtlWnd.Show();
        }

        private void BulkWinClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.RemCtlWndPos = ((RemCtlWnd)sender).RestoreBounds;
            btnStart.IsEnabled = true;
            btnPause.IsEnabled = true;
        }

        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            lstLog.Items.Clear();
        }

        private void btnClearPlot_Click(object sender, RoutedEventArgs e)
        {
            SetupPlotter();
        }

        private void DisplaySensors()
        {
            m_Sensors.Clear();
            wpSensors.Children.Clear();

            foreach (SpecSensor ss in m_specRobot.SensorSpecs)
            {
                Border bdr = new Border();
                bdr.BorderThickness = new Thickness(1);
                bdr.BorderBrush = Brushes.Black;
                bdr.CornerRadius = new CornerRadius(5);
                bdr.Margin = new Thickness(2);
                bdr.Height = 25;

                TextBlock tb = new TextBlock();
                tb.Margin = new Thickness(5);
                tb.Text = ss.Name;

                bdr.Child = tb;
                wpSensors.Children.Add(bdr);

                m_Sensors.Add(ss.Name, tb);
            }
        }

        private void DisplayBehaviors()
        {
            m_Behaviors.Clear();
            spBehaviors.Children.Clear();

            List<SpecBehavior> sblist = m_specRobot.BehaviorSpecs.Values.OrderBy(x => x.Priority).ToList<SpecBehavior>();
            foreach (SpecBehavior sb in sblist)
            {
                Border bdr = new Border();
                bdr.BorderThickness = new Thickness(1);
                bdr.BorderBrush = Brushes.Black;
                bdr.CornerRadius = new CornerRadius(5);
                bdr.Margin = new Thickness(2);
                bdr.Height = 25;

                TextBlock tb = new TextBlock();
                tb.Margin = new Thickness(5);
                tb.Text = sb.Name;

                bdr.Child = tb;
                spBehaviors.Children.Add(bdr);

                m_Behaviors.Add(sb.Name, tb);
            }
        }

        private void SetupPlotter()
        {
            m_Timeline = 0;
            m_Plotter.ClearLineGraphs();
            foreach (SpecSensor ss in m_specRobot.SensorSpecs)
            {
                if (ss.Plot && ss.RetType == typeof(int))
                    m_Plotter.AddLineGraph(new LineGraphData(1000, ss.Name), ss.Name);
            }
        }

        private void UpdateUI(object sender, EventArgs e)
        {
            SetButtonState();

            ActivityLogEntry entry;
            if(m_Robot.ActivityLog.GetEntry(out entry))
            {
                // Display log entry in list box
                if ((((bool)cbMessages.IsChecked) && entry.EntryType == ActivityLogEntry.LogEntryType.Message) ||
                    (((bool)cbErrors.IsChecked) && entry.EntryType == ActivityLogEntry.LogEntryType.Error) ||
                    (((bool)cbSensorData.IsChecked) && entry.EntryType == ActivityLogEntry.LogEntryType.SensorData))
                {
                    TextBlock le = new TextBlock();
                    le.Margin = new Thickness(2);
                    le.TextWrapping = TextWrapping.Wrap;
                    le.Width = lstLog.ActualWidth - 30;
                    le.Text = entry.ToString();

                    if (entry.EntryType == ActivityLogEntry.LogEntryType.Error)
                        le.Background = Brushes.Yellow;

                    lstLog.Items.Add(le);
                    lstLog.ScrollIntoView(lstLog.Items[lstLog.Items.Count - 1]);
                }

                // Display telemetry request/reception time
                if (entry.EntryType == ActivityLogEntry.LogEntryType.Timing)
                {
                    long time = (long)entry.Reference;
                    txtSensorTime.Content = time.ToString() + " ms";
                }

                // Display sensor readings
                if (entry.EntryType == ActivityLogEntry.LogEntryType.SensorData)
                {
                    SensorRepository sr = (SensorRepository)entry.Reference;
                    foreach (SpecSensor ss in m_specRobot.SensorSpecs)
                    {
                        TextBlock tb = m_Sensors[ss.Name];
                        string value;
                        if (ss.RetType == typeof(bool))
                            value = sr.SensorValueBool(ss.Name).ToString();
                        else
                            value = sr.SensorValueStr(ss.Name);
                        tb.Text = ss.Name + " = " + value;

                        if (ss.Plot && ss.RetType == typeof(int))
                            m_Plotter.AddDataPoint(ss.Name, m_Timeline++, double.Parse(value));
                    }
                }

                // Refresh plotter every second or so since it's so damn slow
                if (m_PlotRefreshCount++ == 10)
                {
                    m_Plotter.Refresh();
                    m_PlotRefreshCount = 0;
                }

                // Indicate winning behavior
                foreach (string bName in m_Behaviors.Keys)
                {
                    if (bName == m_Robot.LastBehaviorWinnerName)
                        m_Behaviors[bName].Background = Brushes.Lime;
                    else
                        m_Behaviors[bName].Background = Brushes.White;
                }
            }
        }

        private void SetButtonState()
        {
            btnRemCtl.IsEnabled = !(m_RemCtlWnd != null && m_RemCtlWnd.IsVisible);

            switch (m_Robot.Status)
            {
                case RobotBrain.RunState.Running:
                    btnPause.Content = "Pause";
                    btnStart.IsEnabled = false;
                    btnPause.IsEnabled = true;
                    btnStop.IsEnabled = true;
                    break;

                case RobotBrain.RunState.Stopped:
                    btnPause.Content = "Pause";
                    btnStart.IsEnabled = true;
                    btnPause.IsEnabled = false;
                    btnStop.IsEnabled = false;
                    break;

                case RobotBrain.RunState.Paused:
                    btnPause.Content = "Resume";
                    btnStart.IsEnabled = false;
                    btnPause.IsEnabled = true;
                    btnStop.IsEnabled = true;
                    break;
            }
        }

        private RobotSpecification BuildRobotSpec()
        {
            RobotSpecification specRobot = new RobotSpecification();

            specRobot.RobotID = "R7";
            specRobot.BehaviorsPath = @"C:\Devel\EmergeStudio\BehaviorSet-R7\bin\Debug";

            SpecComm cs1 = new SpecComm() { Name = "Drive", CommunicatorType = SpecComm.CommType.Serial, HasTelemetry = true, HasRemoteControl = true };
            cs1.Parameters.Add("COMPORT", "COM25");
            cs1.Parameters.Add("BAUD", "9600");
            specRobot.CommSpecs.Add(cs1);

            specRobot.SensorSpecs.Add(new SpecSensor { Name = "DistFwd", Position = 0, RetType = typeof(int), Plot = true });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "DistR30", Position = 1, RetType = typeof(int), Plot = true });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "DistL30", Position = 2, RetType = typeof(int), Plot = true });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "DistL90", Position = 3, RetType = typeof(int) });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "Heading", Position = 4, RetType = typeof(int) });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "Encoder1", Position = 5, RetType = typeof(int), Plot = true });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "Encoder2", Position = 6, RetType = typeof(int) });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "Speed", Position = 7, RetType = typeof(int) });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "Direction", Position = 8, RetType = typeof(int) });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "IsPower", Position = 9, RetType = typeof(bool) });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "HasArrived", Position = 10, RetType = typeof(bool) });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "ArmBase", Position = 11, RetType = typeof(int) });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "ArmElbow", Position = 12, RetType = typeof(int) });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "ArmWrist", Position = 13, RetType = typeof(int) });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "ArmWristRot", Position = 14, RetType = typeof(int) });
            specRobot.SensorSpecs.Add(new SpecSensor { Name = "ArmGripper", Position = 15, RetType = typeof(int) });

            SpecBehavior bs = new SpecBehavior() { Name = "Blocked", Priority = 1 };
            bs.Parameters.Add("Parameter1", "abc");
            specRobot.BehaviorSpecs.Add("Blocked", bs);
            specRobot.BehaviorSpecs.Add("Avoid", new SpecBehavior() { Name = "Avoid", Priority = 2 });
            specRobot.BehaviorSpecs.Add("Cruise", new SpecBehavior() { Name = "Cruise", Priority = 3 });

            specRobot.HaltRequests.Add(new Request() { Name = "Halt/Reset", Channel = "Drive", Command = "EH" });
            specRobot.HaltRequests.Add(new Request() { Name = "Power Off", Channel = "Drive", Command = "p1" });

            specRobot.GeneralSpecs.Add("CruiseDistance", "40");

            specRobot.ControlSpecs.Add(new SpecControl() { ControlName = "btnPower2", IsEnabled = false });
            specRobot.ControlSpecs.Add(new SpecControl() { ControlName = "btnPower3", IsEnabled = false });
            specRobot.ControlSpecs.Add(new SpecControl() { ControlName = "btnFunc1", IsEnabled = false });
            specRobot.ControlSpecs.Add(new SpecControl() { ControlName = "btnFunc2", IsEnabled = false });
            specRobot.ControlSpecs.Add(new SpecControl() { ControlName = "btnFunc3", IsEnabled = false });
            specRobot.ControlSpecs.Add(new SpecControl() { ControlName = "btnVidUp", IsEnabled = false });
            specRobot.ControlSpecs.Add(new SpecControl() { ControlName = "btnVidDown", IsEnabled = false });
            specRobot.ControlSpecs.Add(new SpecControl() { ControlName = "btnVidLeft", IsEnabled = false });
            specRobot.ControlSpecs.Add(new SpecControl() { ControlName = "btnVidRight", IsEnabled = false });
            specRobot.ControlSpecs.Add(new SpecControl() { ControlName = "btnVidCenter", IsEnabled = false });
            specRobot.ControlSpecs.Add(new SpecControl() { ControlName = "btnVidScan", IsEnabled = false });

            // Save it
//            specRobot.Save("Test.xml");

            return specRobot;
        }
    }
}
