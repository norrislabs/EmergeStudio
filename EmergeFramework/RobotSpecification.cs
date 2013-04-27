using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace EmergeFramework
{
    public class RobotSpecification
    {
        private List<SpecComm> m_CommSpec = new List<SpecComm>();
        private List<SpecSensor> m_SensorSpecs = new List<SpecSensor>();
        private Dictionary<string, SpecBehavior> m_BehaviorSpecs = new Dictionary<string, SpecBehavior>();
        private Dictionary<string, string> m_GeneralSpecs = new Dictionary<string, string>();
        private List<Request> m_HaltRequests = new List<Request>();
        private List<SpecControl> m_ControlSpecs = new List<SpecControl>();
        private List<SpecRoute> m_RouteSpecs = new List<SpecRoute>();

        public RobotSpecification()
        {
        }

        public RobotSpecification(string filename)
        {
            BehaviorsPath = Path.GetDirectoryName(filename);

            XDocument doc = XDocument.Load(filename);

            // Robot ID
            RobotID = doc.Descendants("RobotID").First().Value;

            // Comm Specs
            var x = doc.Descendants("CommSpecs");
            var specComms = from cs in doc.Descendants("CommSpecs").Descendants("CommSpec") select cs;
            foreach (XElement elSpecComm in specComms)
            {
                SpecComm sc = new SpecComm();
                sc.Name = elSpecComm.Element("Name").Value;
                sc.CommunicatorType = (SpecComm.CommType)Enum.Parse(typeof(SpecComm.CommType), elSpecComm.Element("CommunicatorType").Value, true);
                sc.HasTelemetry = (bool)bool.Parse(elSpecComm.Element("HasTelemetry").Value);
                sc.HasRemoteControl = (bool)bool.Parse(elSpecComm.Element("HasRemoteControl").Value);

                var parameters = from parm in elSpecComm.Descendants("Parameters").Descendants("Parameter") select parm;
                foreach (XElement elParm in parameters)
                    sc.Parameters.Add(elParm.Attribute("Name").Value, elParm.Attribute("Value").Value);

                m_CommSpec.Add(sc);
            }

            // Sensor Specs
            var specSensors = from ss in doc.Descendants("SensorSpecs").Descendants("SensorSpec") select ss;
            foreach (XElement elSpecSensor in specSensors)
            {
                SpecSensor ss = new SpecSensor();
                ss.Name = elSpecSensor.Element("Name").Value;
                ss.Position = (int)int.Parse(elSpecSensor.Element("Position").Value);
                ss.RetType = Type.GetType(elSpecSensor.Element("RetType").Value);
                ss.Plot = (bool)bool.Parse(elSpecSensor.Element("Plot").Value);

                m_SensorSpecs.Add(ss);
            }

            // Behavior Specs
            var specBehaviors = from bs in doc.Descendants("BehaviorSpecs").Descendants("BehaviorSpec") select bs;
            foreach (XElement elSpecBehavior in specBehaviors)
            {
                SpecBehavior bs = new SpecBehavior();
                bs.Name = elSpecBehavior.Element("Name").Value;
                bs.Priority = (int)int.Parse(elSpecBehavior.Element("Priority").Value);

                var parameters = from parm in elSpecBehavior.Descendants("Parameters").Descendants("Parameter") select parm;
                foreach (XElement elParm in parameters)
                    bs.Parameters.Add(elParm.Attribute("Name").Value, elParm.Attribute("Value").Value);

                m_BehaviorSpecs.Add(bs.Name, bs);
            }

            // General Specs
            var specGeneral = from gs in doc.Descendants("GeneralSpecs").Descendants("GeneralSpec") select gs;
            foreach (XElement elGeneral in specGeneral)
                m_GeneralSpecs.Add(elGeneral.Attribute("Name").Value, elGeneral.Attribute("Value").Value);

            // Halt Requests
            var requestsHalt = from hr in doc.Descendants("HaltRequests").Descendants("HaltRequest") select hr;
            foreach (XElement elHaltRequest in requestsHalt)
            {
                Request req = new Request();
                req.Name = elHaltRequest.Element("Name").Value;
                req.Channel = elHaltRequest.Element("Channel").Value;
                req.Command = elHaltRequest.Element("Command").Value;

                m_HaltRequests.Add(req);
            }

            // Control Specs
            var specControls = from hr in doc.Descendants("ControlSpecs").Descendants("ControlSpec") select hr;
            foreach (XElement elSpecControl in specControls)
            {
                SpecControl cs = new SpecControl();
                cs.ControlName = elSpecControl.Element("ControlName").Value;
                cs.Text = elSpecControl.Element("Text").Value;
                cs.IsEnabled = (bool)bool.Parse(elSpecControl.Element("IsEnabled").Value);

                m_ControlSpecs.Add(cs);
            }

            // Route Specs
            var specRoute = from rs in doc.Descendants("RouteSpecs").Descendants("RouteSpec") select rs;
            foreach (XElement elSpecRoute in specRoute)
            {
                SpecRoute rs = new SpecRoute();
                rs.OperatorName = elSpecRoute.Element("OperatorName").Value;
                rs.Parameters = elSpecRoute.Element("Parameters").Value;

                m_RouteSpecs.Add(rs);
            }
        }

        public static RobotSpecification Load(string filename)
        {
            return new RobotSpecification(filename);
        }

        public void Save(string filename)
        {
            XElement doc = new XElement("RobotSpec", new XElement("RobotID", RobotID));

            // Comm Specs
            var specComms = from cs in m_CommSpec
                            select
                                new XElement("CommSpec",
                                    new XElement("Name", cs.Name),
                                    new XElement("CommunicatorType", cs.CommunicatorType),
                                    new XElement("HasTelemetry", cs.HasTelemetry),
                                    new XElement("HasRemoteControl", cs.HasRemoteControl),
                                    new XElement("Parameters", from parm in cs.Parameters.Keys
                                                               select new XElement("Parameter", new XAttribute("Name", parm), new XAttribute("Value", cs.Parameters[parm])))); 
            doc.Add(new XElement("CommSpecs", specComms));

            // Sensor Specs
            var specSensors = from ss in m_SensorSpecs
                              select
                                  new XElement("SensorSpec",
                                      new XElement("Name", ss.Name),
                                      new XElement("Position", ss.Position),
                                      new XElement("RetType", ss.RetType),
                                      new XElement("Plot", ss.Plot));
            doc.Add(new XElement("SensorSpecs", specSensors));

            // Behavior Specs
            var specBehaviors = from bs in m_BehaviorSpecs.Keys
                                select
                                    new XElement("BehaviorSpec",
                                        new XElement("Name", m_BehaviorSpecs[bs].Name),
                                        new XElement("Priority", m_BehaviorSpecs[bs].Priority),
                                        new XElement("Parameters", from parm in m_BehaviorSpecs[bs].Parameters.Keys
                                                                   select new XElement("Parameter", new XAttribute("Name", parm), new XAttribute("Value", m_BehaviorSpecs[bs].Parameters[parm])))); 
            doc.Add(new XElement("BehaviorSpecs", specBehaviors));

            // General Specs
            var specGeneral = from gs in m_GeneralSpecs.Keys
                                select
                                    new XElement("GeneralSpec", new XAttribute("Name", gs), new XAttribute("Value", m_GeneralSpecs[gs]));
            doc.Add(new XElement("GeneralSpecs", specGeneral));

            // Halt Requests
            var requestsHalt = from hr in m_HaltRequests
                              select
                                  new XElement("HaltRequest",
                                      new XElement("Name", hr.Name),
                                      new XElement("Channel", hr.Channel),
                                      new XElement("Command", hr.Command));
            doc.Add(new XElement("HaltRequests", requestsHalt));

            // Control Specs
            var specControls = from cs in m_ControlSpecs
                              select
                                  new XElement("ControlSpec",
                                      new XElement("ControlName", cs.ControlName),
                                      new XElement("Text", cs.Text),
                                      new XElement("IsEnabled", cs.IsEnabled));
            doc.Add(new XElement("ControlSpecs", specControls));

            // Route Specs
            var specRoute = from rs in m_RouteSpecs
                              select
                                  new XElement("RouteSpec",
                                      new XElement("OperatorName", rs.OperatorName),
                                      new XElement("Parameters", rs.Parameters));
            doc.Add(new XElement("RouteSpecs", specRoute));

            // Write out the XML to a file
            doc.Save(filename);
        }

        public string RobotID { get; set; }

        public string BehaviorsPath { get; set; }

        public List<SpecComm> CommSpecs
        {
            get { return m_CommSpec; }
        }
        
        public List<SpecSensor> SensorSpecs
        {
            get { return m_SensorSpecs; }
        }

        public Dictionary<string, SpecBehavior> BehaviorSpecs
        {
            get { return m_BehaviorSpecs; }
        }

        public List<Request> HaltRequests
        {
            get { return m_HaltRequests; }
        }

        public Dictionary<string, string> GeneralSpecs
        {
            get { return m_GeneralSpecs; }
        }

        public List<SpecControl> ControlSpecs
        {
            get { return m_ControlSpecs; }
        }

        public List<SpecRoute> RouteSpecs
        {
            get { return m_RouteSpecs; }
        }
    }
}
