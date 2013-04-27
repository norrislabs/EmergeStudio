using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace EmergeFramework.Communicators
{
    class SerialComm : CommLink
    {
        private SerialPort m_Port;
        private string m_RobotID;
        private object m_lockObj = new object();

        public override void Initialize(string Name, string RobotID, Dictionary<string, string> Parameters)
        {
            base.Initialize(Name, RobotID, Parameters);

            m_RobotID = RobotID;

            m_Port = new SerialPort();
            m_Port.PortName = Parameters["COMPORT"];
            m_Port.BaudRate = int.Parse(Parameters["BAUD"]);

            m_Port.Encoding = Encoding.UTF8;
            m_Port.DataBits = 8;
            m_Port.StopBits = StopBits.One;
            m_Port.Parity = Parity.None;
            m_Port.Handshake = Handshake.None;
            m_Port.WriteTimeout = 500;
            m_Port.ReadTimeout = 5000;
            m_Port.NewLine = "\r";
        }

        public override void Start()
        {
             m_Port.Open();
             m_Port.DiscardInBuffer();
        }

        public override void Stop()
        {
            m_Port.DiscardInBuffer();
            m_Port.Close();
        }

        public override string ReadLine()
        {
            try
            {
                return m_Port.ReadLine();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public override void Flush()
        {
            m_Port.DiscardInBuffer();
        }

        public override void Send(string data)
        {
            lock (m_lockObj)
            {
                m_Port.WriteLine(">" + m_RobotID + data);
            }
        }

        public override void Send(RequestQueue requests)
        {
            lock (m_lockObj)
            {
                foreach (Request request in requests)
                {
                    m_Port.WriteLine(">" + m_RobotID + request.Command);
                }
            }
        }
    }
}
