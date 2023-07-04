using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Databases.DelayedOperations;
using Data_Management_for_Unity.Runtime.Databases.Structs;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        private readonly Dictionary<string, Queue<DelayedOperation>> _delayedOperations =
            new Dictionary<string, Queue<DelayedOperation>>();

        private void EnqueueDelayedOperation(string id, DelayedOperation operation)
        {
            lock (_delayedOperations)
            {
                //create queue if necessary
                if (!_delayedOperations.TryGetValue(id, out Queue<DelayedOperation> delayed))
                {
                    delayed = new Queue<DelayedOperation>();
                    _delayedOperations.Add(id, delayed);
                }
                
                //enqueue element
                delayed.Enqueue(operation);
            }
        }

        /// <summary>
        /// Tries to find delayed set requests with required modCount
        /// </summary>
        private bool TryDequeueDelayedOperation(string id, int modCount, out DelayedOperation operation)
        {
            //init default value
            operation = default;
            
            lock (_delayedOperations)
            {
                //no delayed sets for id exist
                if (!_delayedOperations.TryGetValue(id, out Queue<DelayedOperation> delayedSets))
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