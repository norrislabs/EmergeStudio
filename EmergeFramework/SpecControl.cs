using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergeFramework
{
    public class SpecControl
    {
        private bool m_IsEnabled = true;

        public string ControlName { get; set; }
        public string Text { get; set; }
        public bool IsEnabled
        {
            get { return m_IsEnabled; }
            set { m_IsEnabled = value; }
        }
    }
}
