using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergeFramework
{
    public abstract class CommLink
    {
        protected string m_Name;

        virtual public void Initialize(string Name, string RobotID, Dictionary<string, string> Parameters)
        {
            m_Name = Name;
        }

        abstract public void Start();
        abstract public void Stop();
        abstract public void Flush();
        abstract public string ReadLine();
        abstract public void Send(string data);
        abstract public void Send(RequestQueue requests);

        public string Name
        {
            get { return m_Name; }
        }
    }
}
