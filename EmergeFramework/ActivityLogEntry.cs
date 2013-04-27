using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergeFramework
{
    public class ActivityLogEntry
    {
        public enum LogEntryType
        {
            Message,
            Error,
            SensorData,
            Timing
        }

        private DateTime m_Timestamp;
        private LogEntryType m_Type;
        private string m_Msg;
        private object m_Ref;

        public ActivityLogEntry(LogEntryType typeEntry, string Message, object Reference)
        {
            m_Timestamp = DateTime.Now;
            m_Type = typeEntry;
            m_Msg = Message;
            m_Ref = Reference;
        }

        public ActivityLogEntry(string Message)
        {
            m_Timestamp = DateTime.Now;
            m_Type = LogEntryType.Message;
            m_Msg = Message;
            m_Ref = null;
        }

        public ActivityLogEntry(string Message, long msTime)
        {
            m_Timestamp = DateTime.Now;
            m_Type = LogEntryType.Timing;
            m_Msg = Message;
            m_Ref = msTime;
        }

        public LogEntryType EntryType
        {
            get { return m_Type; }
        }

        public string Message
        {
            get { return m_Msg; }
        }

        public object Reference
        {
            get { return m_Ref; }
        }

        public DateTime Timestamp
        {
            get { return m_Timestamp; }
        }

        public override string ToString()
        {
            if (m_Type == ActivityLogEntry.LogEntryType.SensorData)
                return ((SensorRepository)m_Ref).ToString();
            else if (m_Type == ActivityLogEntry.LogEntryType.Error)
            {
                if (m_Ref != null)
                    return ((Exception)m_Ref).Message;
                else
                    return m_Msg;
            }
            else
                return m_Msg;
        }
    }
}
