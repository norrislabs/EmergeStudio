using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmergeFramework;

namespace BehaviorSet_R7
{
    public class Avoid : Behavior
    {
        public override void Initialize(RobotSpecification specRobot, ActivityLog logActivity)
        {
            base.Initialize(specRobot, logActivity);
        }

        public override RequestQueue Execute(SensorRepository repSensors, RequestQueue LastWinner)
        {
            bool iWon = LastWinner != null && LastWinner.BehaviorName == m_Name;
            RequestQueue requests = new RequestQueue(m_Name);

            if (repSensors.SensorValueBool("IsPower") && repSensors.SensorValueInt("Direction") == (int)Cruise.MoveDir.Mov_Fwd)
            {
                if(repSensors.SensorValueInt("DistL30") < repSensors.SensorValueInt("DistR30"))
                {
                    if(repSensors.SensorValueInt("DistL30") < 20 || (iWon && repSensors.SensorValueInt("DistL30") < 24))
                    {
                        requests.Enqueue(new Request() { Name = "Turn Right", Channel = "Drive", Command = "RT" });
                    }
                }
                else
                {
                    if(repSensors.SensorValueInt("DistR30") < 20 || (iWon && repSensors.SensorValueInt("DistR30") < 24))
                    {
                        requests.Enqueue(new Request() { Name = "Turn Left", Channel = "Drive", Command = "LF" });
                    }
                }
            }

            return requests;
        }
    }
}
