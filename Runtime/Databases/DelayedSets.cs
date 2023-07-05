using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Databases.DelayedOperations;
using Data_Management_for_Unity.Runtime.Databases.Structs;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        private readonly Dictionary<string, Queue<SynchronisedOperation>> _delayedOperations =
            new Dictionary<string, Queue<SynchronisedOperation>>();

        private void EnqueueDelayedOperation(string id, SynchronisedOperation operation)
        {
            lock (_delayedOperations)
            {
                //create queue if necessary
                if (!_delayedOperations.TryGetValue(id, out Queue<SynchronisedOperation> delayed))
                {
                    delayed = new Queue<SynchronisedOperation>();
                    _delayedOperations.Add(id, delayed);
                }
                
                //enqueue element
                delayed.Enqueue(operation);
            }
        }

        /// <summary>
        /// Tries to find delayed set requests with required modCount
        /// </summary>
        private bool TryDequeueDelayedOperation(string id, int modCount, out SynchronisedOperation operation)
        {
            //init default value
            operation = default;
            
            lock (_delayedOperations)
            {
                //no delayed sets for id exist
                if (!_delayedOperations.TryGetValue(id, out Queue<SynchronisedOperation> delayedSets))
                    return false;

                //no delayed sets for id exist
                if (!delayedSets.TryPeek(out operation)) return false;

                //requests mod count must meet expected
                if (operation.ModCount != modCount) return false;

                delayedSets.Dequeue();
                return true;
            }
        }
    }
}