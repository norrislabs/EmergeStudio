using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmergeFramework;
using BehaviorSet_R7.NavOperators;

namespace BehaviorSet_R7
{
    public class NavRoute : Behavior
    {
        List<NavOperator> m_Route;
        int m_InstPtr;
        bool m_IsFirst;

        public override void Initialize(RobotSpecification specRobot, ActivityLog logActivity)
        {
            base.Initialize(specRobot, logActivity);

            m_Route = new List<NavOperator>();
            m_InstPtr = 0;
            m_IsFirst = true;

            // Load route operators
            foreach (SpecRoute sr in specRobot.RouteSpecs)
            {
                Type typeOp = Type.GetType("BehaviorSet_R7.NavOperators." + sr.OperatorName);
                NavOperator op = (NavOperator)Activator.CreateInstance(typeOp);
                op.Initialize(sr.OperatorName, logActivity, String2IntArray(sr.Parameters));
                m_Route.Add(op);
            }
        }

        public override RequestQueue Execute(SensorRepository repSensors, RequestQueue LastWinner)
        {
            bool iWon = LastWinner != null && LastWinner.BehaviorName == m_Name;
            RequestQueue requests = new RequestQueue(m_Name);

            if (m_InstPtr < m_Route.Count)
            {
                NavOperator.IPAction action;
                if (m_IsFirst)
                    action = m_Route[m_InstPtr].ExecuteFirst(repSensors, requests, iWon);
                else
                    action = m_Route[m_InstPtr].ExecuteRest(repSensors, requests, iWon);

                switch (action)
                {
                    case NavOperator.IPAction.Increment:
                        m_InstPtr++;
                        m_IsFirst = true;
                        break;

                    case NavOperator.IPAction.Reset:
                        m_InstPtr = 0;
                        m_IsFirst = true;
                        break;

                    case NavOperator.IPAction.DoFirst:
                        m_IsFirst = true;
                        break;

                    case NavOperator.IPAction.DoRest:
                        m_IsFirst = false;
                        break;
                }
            }

            return requests;
        }

        private int[] String2IntArray(string ints)
        {
            string[] items = ints.Split(',');
            List<int> parms = new List<int>();

            foreach (string item in items)
            {
                if(!string.IsNullOrEmpty(item))
                    parms.Add(int.Parse(item));
            }

            return parms.ToArray();
        }
    }
}
