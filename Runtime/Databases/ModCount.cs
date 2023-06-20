using System.Collections.Generic;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        private readonly Dictionary<string, int> _modCount = new Dictionary<string, int>();

        public int GetModCount(string id)
        {
            lock (_modCount)
            {
                return _modCount.TryGetValue(id, out int modCount) ? modCount : 0;
            }
        }

        /// <summary>
        /// Increments the current modification count by one and returns it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int IncrementModCount(string id)
        {
            lock (_modCount)
            {
                return _modCount.TryGetValue(id, out int modCount) ? _modCount[id] = modCount + 1 : _modCount[id] = 1;
            }
        }
    }
}