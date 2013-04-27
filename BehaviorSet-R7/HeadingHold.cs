using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmergeFramework;

namespace BehaviorSet_R7
{
    public class HeadingHold : Behavior
    {
        int m_DesiredHeading;
        PID m_HhPID;

        public override void Initialize(RobotSpecification specRobot, ActivityLog logActivity)
        {
            base.Initialize(specRobot, logActivity);
  
            // Initialize Heading Hold PID
            m_HhPID = new PID(1, 0, 0, 0, -80, 80);

            if (specRobot.GeneralSpecs.ContainsKey("DesiredHeading"))
                m_DesiredHeading = int.Parse(specRobot.GeneralSpecs["DesiredHeading"]);
            else
                m_DesiredHeading = -1;
        }

        public override RequestQueue Execute(SensorRepository repSensors, RequestQueue LastWinner)
        {
            bool iWon = LastWinner != null && LastWinner.BehaviorName == m_Name;
            RequestQueue requests = new RequestQueue(m_Name);

            if (repSensors.SensorValueBool("IsPower") && repSensors.SensorValueInt("Direction") == (int)Cruise.MoveDir.Mov_Fwd && m_DesiredHeading != -1)
            {
                int herr = HeadingError(repSensors.SensorValueInt("Heading"));
                int corr = m_HhPID.Calculate(herr);

                if(corr > 0)
                    requests.Enqueue(new Request() { Name = "Right Turn" + corr.ToString(), Channel = "Drive", Command = "RX" + corr.ToString() });
                else if (corr < 0)
                    requests.Enqueue(new Request() { Name = "Left Turn" + Math.Abs(corr).ToString(), Channel = "Drive", Command = "LX" + Math.Abs(corr).ToString() });
            }

            return requests;
        }

        private int HeadingError(int currentHeading)
        {
            int err = currentHeading - m_DesiredHeading;

            // Handle wrap-around
            if(err < -180)
                err = err + 360;
            else if (err > 180)
                err = err - 360;

            return err;
        }
    }
}
