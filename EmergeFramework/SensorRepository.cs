using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergeFramework
{
    public class SensorRepository
    {
        private RobotSpecification m_specRobot;
        private Dictionary<string, string> m_Values = new Dictionary<string, string>();
        private bool m_Valid = false;

        internal SensorRepository(RobotSpecification specRobot)
        {
            m_specRobot = specRobot;
        }

        public long ID { get; set; }

        public long AcquisitionTime { get; set; }

        public bool IsValid
        {
            get { return m_Valid; }
        }

        public void ParseSensorData(string data, int countItems)
        {
            m_Valid = false;
            if (data.Length > 0)
            {
                string[] readings = data.Substring(3).Split(',');
                if (readings.Length == countItems)
                {
                    foreach (SpecSensor ss in m_specRobot.SensorSpecs)
                        m_Values.Add(ss.Name, readings[ss.Position]);
                    m_Valid = true;
                }
            }
        }

        public int SensorValueInt(string key)
        {
            return int.Parse(m_Values[key]);
        }

        public bool SensorValueBool(string key)
        {
            if (m_Values[key] == "1" || m_Values[key].ToLower() == "true" || m_Values[key].ToLower() == "yes")
                return true;
            else
                return false;
        }

        public string SensorValueStr(string key)
        {
            return m_Values[key];
        }

        public override string ToString()
        {
            if (IsValid)
            {
                StringBuilder sb = new StringBuilder();
                foreach (SpecSensor ss in m_specRobot.SensorSpecs)
                {
                    if (ss.RetType == typeof(int))
                        sb.Append(ss.Name + "=" + SensorValueInt(ss.Name).ToString() + ", ");
                    else if (ss.RetType == typeof(string))
                        sb.Append(ss.Name + "=" + SensorValueInt(ss.Name) + ", ");
                    else if (ss.RetType == typeof(bool))
                        sb.Append(ss.Name + "=" + SensorValueBool(ss.Name) + ", ");
                }
                return sb.ToString().TrimEnd(',');
            }
            return "";
        }
    }
}
