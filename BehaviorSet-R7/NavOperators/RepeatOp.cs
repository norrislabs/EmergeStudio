using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmergeFramework;

namespace BehaviorSet_R7.NavOperators
{
    public class RepeatOp : NavOperator
    {
        public override void Initialize(string opName, ActivityLog logActivity, params int[] Parameters)
        {
            base.Initialize(opName, logActivity, Parameters);
        }

        public override IPAction ExecuteFirst(SensorRepository repSensors, RequestQueue requests, bool iWon)
        {
            return IPAction.Reset;
        }

        public override IPAction ExecuteRest(SensorRepository repSensors, RequestQueue requests, bool iWon)
        {
            throw new NotImplementedException();
        }
    }
}
