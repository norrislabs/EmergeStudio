using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergeFramework.Communicators
{
    class TcpComm : CommLink
    {
        public override void Initialize(string Name, string RobotID, Dictionary<string, string> Parameters)
        {
            base.Initialize(Name, RobotID, Parameters);
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override void Send(string data)
        {
            throw new NotImplementedException();
        }

        public override void Send(RequestQueue requests)
        {
            throw new NotImplementedException();
        }
    }
}
