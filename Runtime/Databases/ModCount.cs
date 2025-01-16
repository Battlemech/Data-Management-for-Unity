using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Databases.Structs;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        /// <summary>
        /// Clients local mod count, usually exceeding that of the server
        /// </summary>
        private readonly Dictionary<string, int> _localModCount = new Dictionary<string, int>();

        /// <summary>
        /// Tracks latest data confirmed by server
        /// </summary>
        private readonly Dictionary<string, ValueRecord> _confirmed = new Dictionary<string, ValueRecord>();

        public int GetModCount(string id)
        {
            lock (_localModCount)
            {
                return _localModCount.GetValueOrDefault(id, -1);
            }
        }

        /// <summary>
        /// Increments the current modification count by one and returns it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int IncrementModCount(string id)
        {
            lock (_localModCount)
            {
                return _localModCount.TryGetValue(id, out int modCount) ? _localModCount[id] = modCount + 1 : _localModCount[id] = 1;
            }
        }

        private bool UpdateConfirmedData(string id, int modCount, byte[] value, Type type)
        {
            //update last local value confirmed by server
            lock (_confirmed)
            {
                if (_confirmed.TryGetValue(id, out ValueRecord confirmed))
                {
                    //more up to date value is already saved locally
                    if (confirmed.ModCount >= modCount) return false;
                }

                _confirmed[id] = new ValueRecord(value, type, modCount);
            }

            //value was updated successfully
            return true;
        }
    }
}