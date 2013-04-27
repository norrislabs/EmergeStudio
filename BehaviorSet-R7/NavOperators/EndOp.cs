using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmergeFramework;

namespace BehaviorSet_R7.NavOperators
{
    public class EndOp : NavOperator
    {
        public override void Initialize(string opName, ActivityLog logActivity, params int[] Parameters)
        {
            base.Initialize(opName, logActivity, Parameters);
        }

        public override NavOperator.IPAction ExecuteFirst(SensorRepository repSensors, RequestQueue requests, bool iWon)
        {
            if (!iWon)
            {
                // Halt
                requests.Enqueue(new Request() { Name = "Halt", Channel = "Drive", Command = "HL" });
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
