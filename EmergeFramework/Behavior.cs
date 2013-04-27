using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergeFramework
{
    public abstract class Behavior
    {
        protected string m_Name;
        protected int m_Priority;
        protected RobotSpecification m_specRobot;
        protected ActivityLog m_logActivity;

        virtual public void Initialize(RobotSpecification specRobot, ActivityLog logActivity)
        {
            m_Name = this.GetType().Name;
            m_Priority = specRobot.BehaviorSpecs[m_Name].Priority;

            m_specRobot = specRobot;
            m_logActivity = logActivity;
        }

        abstract public RequestQueue Execute(SensorRepository repSensors, RequestQueue LastWinner);

        public string Name
        {
            get { return m_Name; }
        }

        public int Priority
        {
            get { return m_Priority; }
        }
    }
}
