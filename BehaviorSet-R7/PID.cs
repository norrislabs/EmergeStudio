using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviorSet_R7
{
    public class PID
    {
        private int m_P, m_I, m_D;
        private int m_Kp, m_Ki, m_Kd;
        private int m_SetPoint;
        private int m_OutMin, m_OutMax;
        private int m_Integral;
        private int m_Previous;

        public PID(int Kp, int Ki, int Kd, int SetPoint, int OutMin, int OutMax)
        {
            m_Kp = Kp;
            m_Ki = Ki;
            m_Kd = Kd;

            m_SetPoint = SetPoint;

            m_OutMin = OutMin;
            m_OutMax = OutMax;
        }

        public void Reset()
        {
            m_Integral = 0;
            m_Previous = 0;
        }

        public int Calculate(int Actual)
        {
            // Calculate error
            int error = m_SetPoint - Actual;

            // Calculate proportional term
            m_P = m_Kp * error;

            // Calculate integral term
            m_Integral = m_Integral + error;
            m_I = m_Ki * m_Integral;
  
            // Calculate derivative term
            m_D = m_Kd * (error - m_Previous);

            // Save error for the next time
            m_Previous = error;  

            // Calculate output
            int pid = m_P + m_I + m_D;
            return Math.Min(Math.Max(pid, m_OutMin), m_OutMax);
        }

        public int P
        {
            get { return m_P; }
        }

        public int I
        {
            get { return m_I; }
        }

        public int D
        {
            get { return m_D; }
        }
    }
}
