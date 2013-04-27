using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmergeFramework;

namespace BehaviorSet_R7
{
    public abstract class NavOperator
    {
        public enum IPAction
        {
            Increment,
            Reset,
            DoFirst,
            DoRest
        }

        protected string m_opName;
        protected ActivityLog m_logActivity;
        protected int[] m_Parameters;

        virtual public void Initialize(string opName, ActivityLog logActivity, params int[] Parameters)
        {
            m_opName = opName;
            m_logActivity = logActivity;
            m_Parameters = Parameters;
        }

        abstract public IPAction ExecuteFirst(SensorRepository repSensors, RequestQueue requests, bool iWon);

        abstract public IPAction ExecuteRest(SensorRepository repSensors, RequestQueue requests, bool iWon);

        public string OperatorName
        {
            get { return m_opName; }
        }

        public int[] Parameters
        {
            get { return m_Parameters; }
        }
    }
}
