using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace EmergeFramework
{
    public class ActivityLog
    {
        private BlockingCollection<ActivityLogEntry> m_LogEntries;

        internal ActivityLog()
        {
            Clear();
        }

        public bool AddEntry(ActivityLogEntry entry)
        {
            return m_LogEntries.TryAdd(entry, 0);
        }

        public bool GetEntry(out ActivityLogEntry entry)
        {
            return m_LogEntries.TryTake(out entry, 0);
        }

        public void Clear()
        {
            ConcurrentQueue<ActivityLogEntry> queue = new ConcurrentQueue<ActivityLogEntry>();
            m_LogEntries = new BlockingCollection<ActivityLogEntry>(queue);
        }
    }
}
