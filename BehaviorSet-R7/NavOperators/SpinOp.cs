using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmergeFramework;

namespace BehaviorSet_R7.NavOperators
{
    public class SpinOp : NavOperator
    {
        private int m_DesiredHeading;
        private int m_RequestedCorrection;

        public override void Initialize(string opName, ActivityLog logActivity, params int[] Parameters)
        {
            base.Initialize(opName, logActivity, Parameters);

            m_DesiredHeading = m_Parameters[0];
        }

        public override IPAction ExecuteFirst(SensorRepository repSensors, RequestQueue requests, bool iWon)
        {
            if (!iWon)
            {
                requests.Enqueue(new Request() { Name = "Halt/Reset", Channel = "Drive", Command = "EH" });

                // Calculate how many degrees to spin to get to desired heading
                int herr = HeadingError(repSensors.SensorValueInt("Heading"));
                m_RequestedCorrection = Math.Abs(herr);
                if (herr > 0)
                    requests.Enqueue(new Request() { Name = "SpinCCW " + m_RequestedCorrection.ToString(), Channel = "Drive", Command = "LX" + m_RequestedCorrection.ToString() });
                else if (herr < 0)
                    requests.Enqueue(new Request() { Name = "SpinCW " + m_RequestedCorrection.ToString(), Channel = "Drive", Command = "RX" + m_RequestedCorrection.ToString() });
                else
                    return IPAction.Increment;

                return IPAction.DoFirst;
            }
            else
                return IPAction.DoRest;
        }

        public override IPAction ExecuteRest(SensorRepository repSensors, RequestQueue requests, bool iWon)
        {
            if (repSensors.SensorValueInt("Direction") == (int)Cruise.MoveDir.Mov_Halt)
                return IPAction.Increment;
            else
                return IPAction.DoRest;
        }

        private int HeadingError(int currentHeading)
        {
            int err = currentHeading - m_DesiredHeading;

            // Handle wrap-around
            if (err < -180)
                err = err + 360;
            else if (err > 180)
                err = err - 360;

            return err;
        }
    }
}
