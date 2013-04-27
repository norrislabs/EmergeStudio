using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmergeFramework;

namespace BehaviorSet_R7.NavOperators
{
    public class FwdOp : NavOperator
    {
        private int m_DesiredDistance;
        private int m_RequestedDistance;
        private int m_DesiredHeading;
        PID m_HhPID;

        public override void Initialize(string opName, ActivityLog logActivity, params int[] Parameters)
        {
            base.Initialize(opName, logActivity, Parameters);

            // Initialize Heading Hold PID
            m_HhPID = new PID(1, 0, 0, 0, -80, 80);

            m_DesiredDistance = m_Parameters[0];
            m_RequestedDistance = m_Parameters[0];
            m_DesiredHeading = m_Parameters[1];
        }

        public override IPAction ExecuteFirst(SensorRepository repSensors, RequestQueue requests, bool iWon)
        {
            if (!iWon)
            {
                requests.Enqueue(new Request() { Name = "Halt/Reset", Channel = "Drive", Command = "EH" });
                requests.Enqueue(new Request() { Name = "Fwd " + m_RequestedDistance, Channel = "Drive", Command = "FX" + m_RequestedDistance });
                return IPAction.DoFirst;
            }
            else
                return IPAction.DoRest;
        }

        public override IPAction ExecuteRest(SensorRepository repSensors, RequestQueue requests, bool iWon)
        {
            int currentDistance = repSensors.SensorValueInt("Encoder1");
            if (currentDistance == m_RequestedDistance)
            {
                // All done, move to next operator
                m_RequestedDistance = m_DesiredDistance;
                return IPAction.Increment;
            }
            else if (repSensors.SensorValueInt("Direction") == (int)Cruise.MoveDir.Mov_Halt && currentDistance > 0)
            {
                // Something stopped us so start over and request the remaining distance
                m_RequestedDistance = m_DesiredDistance - currentDistance;
                return IPAction.DoFirst;
            }
            else if (repSensors.SensorValueInt("Direction") == (int)Cruise.MoveDir.Mov_Fwd && m_DesiredHeading != -1)
            {
                // Heading hold
                int herr = HeadingError(repSensors.SensorValueInt("Heading"));
                int corr = m_HhPID.Calculate(herr);

                if (corr > 0)
                    requests.Enqueue(new Request() { Name = "Right Turn" + corr.ToString(), Channel = "Drive", Command = "RX" + corr.ToString() });
                else if (corr < 0)
                    requests.Enqueue(new Request() { Name = "Left Turn" + Math.Abs(corr).ToString(), Channel = "Drive", Command = "LX" + Math.Abs(corr).ToString() });
            }

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
