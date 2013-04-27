using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergeFramework
{
    public class RequestQueue : IEnumerable<Request>
    {
        private string m_Name;
        private Queue<Request> m_Requests = new Queue<Request>();

        public RequestQueue(string BehaviorName)
        {
            m_Name = BehaviorName;
        }

        public void Enqueue(Request request)
        {
            m_Requests.Enqueue(request);
        }

        public Request Dequeue()
        {
            return m_Requests.Dequeue();
        }

        public int Count
        {
            get { return m_Requests.Count; }
        }

        public void Clear()
        {
            m_Requests.Clear();
        }

        public bool Contains(Request request)
        {
            return m_Requests.Contains(request);
        }

        public string BehaviorName
        {
            get { return m_Name; }
        }

        public IEnumerator<Request> GetEnumerator()
        {
            return m_Requests.GetEnumerator();
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
