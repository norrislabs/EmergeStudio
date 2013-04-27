using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmergeFramework;

namespace BehaviorSet_R7
{
    public class Cruise : Behavior
    {
        public enum MoveDir
        {
            Mov_Halt = 0,
            Mov_Fwd  = 1,
            Mov_Rev  = 2,
            Mov_SpinCW = 3,
            Mov_SpinCCW = 4
        }

        private string m_CruiseDistance = "50";

        public override void Initialize(RobotSpecification specRobot, ActivityLog logActivity)
        {
            base.Initialize(specRobot, logActivity);

            if (specRobot.GeneralSpecs.ContainsKey("CruiseDistance"))
                m_CruiseDistance = specRobot.GeneralSpecs["CruiseDistance"];
        }

        public override RequestQueue Execute(SensorRepository repSensors, RequestQueue LastWinner)
        {
            bool iWon = LastWinner != null && LastWinner.BehaviorName == m_Name;
            RequestQueue requests = new RequestQueue(m_Name);

            if (!iWon && repSensors.SensorValueInt("Direction") == (int)Cruise.MoveDir.Mov_Halt)
            {
                requests.Enqueue(new Request() { Name = "Halt/Reset", Channel = "Drive", Command = "EH" });
                requests.Enqueue(new Request() { Name = "Fwd " + m_CruiseDistance, Channel = "Drive", Command = "FX" + m_CruiseDistance });
            }

            return requests;
        }
    }
}
