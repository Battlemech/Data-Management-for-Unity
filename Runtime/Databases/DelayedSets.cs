using System;
using System.Collections.Generic;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        private readonly Dictionary<string, Queue<ConfirmedValue>> _delayedSets =
            new Dictionary<string, Queue<ConfirmedValue>>();

        private void EnqueueDelayedSet(string id, byte[] value, Type type, int modCount)
        {
            lock (_delayedSets)
            {
                //create queue if necessary
                if (!_delayedSets.TryGetValue(id, out Queue<ConfirmedValue> delayed))
                {
                    delayed = new Queue<ConfirmedValue>();
                    _delayedSets.Add(id, delayed);
                }
                
                //enqueue element
                delayed.Enqueue(new ConfirmedValue(value, type, modCount));
            }
        }

        /// <summary>
        /// Tries to find delayed set requests with required modCount
        /// </summary>
        private bool TryDequeueDelayedSet(string id, int modCount, out ConfirmedValue value)
        {
            //init default value
            value = default;
            
            lock (_delayedSets)
            {
                //no delayed sets for id exist
                if (!_delayedSets.TryGetValue(id, out Queue<ConfirmedValue> delayedSets))
                    return false;

                //no delayed sets for id exist
                if (!delayedSets.TryPeek(out value)) return false;

                //requests mod count must meet expected
                return value.ModCount == modCount;
            }
        }
    }
}