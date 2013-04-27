using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmergeFramework;

namespace BehaviorSet_R7
{
    public class Blocked : Behavior
    {
        public override void Initialize(RobotSpecification specRobot, ActivityLog logActivity)
        {
            base.Initialize(specRobot, logActivity);
        }

        public override RequestQueue Execute(SensorRepository repSensors, RequestQueue LastWinner)
        {
            bool iWon = LastWinner != null && LastWinner.BehaviorName == m_Name;
            RequestQueue requests = new RequestQueue(m_Name);

            // Ignore if powered off or during a spin
            if (!repSensors.SensorValueBool("IsPower") ||
                repSensors.SensorValueInt("Direction") == (int)Cruise.MoveDir.Mov_SpinCW ||
                repSensors.SensorValueInt("Direction") == (int)Cruise.MoveDir.Mov_SpinCCW)
                return requests;

            if (repSensors.SensorValueInt("DistFwd") <= 8)
            {
                if (iWon || repSensors.SensorValueInt("Direction") == (int)Cruise.MoveDir.Mov_Halt)
                {
                    // Halt already sent so create a NOP request to retain control
                    requests.Enqueue(new Request() { Name = "NOP", Channel = "NA", Command = "NP" });
                }
                else
                {
                    // Halt
                    requests.Enqueue(new Request() { Name = "Halt", Channel = "Drive", Command = "HL" });
                }
            }

            return requests;
        }
    }
}
