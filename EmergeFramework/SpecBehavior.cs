using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergeFramework
{
    public class SpecBehavior
    {
        private Dictionary<string, string> m_Parameters = new Dictionary<string, string>();

        public string Name { get; set; }

        public int Priority { get; set; }

        public Dictionary<string,string> Parameters
        {
            get { return m_Parameters; }
        }
    }
}
