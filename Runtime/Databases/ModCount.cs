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

        /// <summary>
        /// Updates the local modCount to newly received value
        /// </summary>
        /// <returns>True if the new modCount was higher than the previously known one</returns>
        private bool UpdateModCount(string id, int modCount)
        {
            lock (_modCount)
            {
                //no local reference of mod count: New modCount is higher
                if (!_modCount.TryGetValue(id, out int knownCount)) return true;

                //local modCount is lower
                if (modCount <= knownCount) return false;

                //update local modCount, remote value is higher
                _modCount[id] = modCount;
                return true;
            }
        }
    }
}