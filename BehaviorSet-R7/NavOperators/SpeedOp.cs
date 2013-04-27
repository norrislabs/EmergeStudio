using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmergeFramework;

namespace BehaviorSet_R7.NavOperators
{
    public class SpeedOp : NavOperator
    {
        private int m_Speed;

        public override void Initialize(string opName, ActivityLog logActivity, params int[] Parameters)
        {
            base.Initialize(opName, logActivity, Parameters);

            m_Speed = Parameters[0];
        }

        public override NavOperator.IPAction ExecuteFirst(SensorRepository repSensors, RequestQueue requests, bool iWon)
        {
            if (!iWon)
            {
                requests.Enqueue(new Request() { Name = "Set Speed" + m_Speed.ToString(), Channel = "Drive", Command = "S" + m_Speed.ToString() });
                return IPAction.DoFirst;
            }
            else
                return IPAction.Increment;
        }

        public override NavOperator.IPAction ExecuteRest(SensorRepository repSensors, RequestQueue requests, bool iWon)
        {
            throw new NotImplementedException();
        }
    }
}
