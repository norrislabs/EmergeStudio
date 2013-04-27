using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergeFramework
{
    public class SpecComm
    {
        private Dictionary<string, string> m_Parameters = new Dictionary<string, string>();
        private bool m_Telemetry = false;
        private bool m_RemCtl = false;

        public enum CommType
        {
            Serial,
            TCP
        }

        public string Name { get; set; }

        public CommType CommunicatorType { get; set; }

        public bool HasTelemetry
        {
            get { return m_Telemetry; }
            set { m_Telemetry = value; }
        }

        public bool HasRemoteControl
        {
            get { return m_RemCtl; }
            set { m_RemCtl = value; }
        }

        public Dictionary<string, string> Parameters
        {
            get { return m_Parameters; }
        }
    }
}
