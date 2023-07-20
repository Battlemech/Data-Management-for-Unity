using System.Collections.Generic;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Server
{
    public partial class SynchronisedSession
    {
        /// <summary>
        /// Tracks the modification count of expected delayed requests
        /// </summary>
        private readonly Dictionary<ValueReference, Queue<int>> _failedSets =
            new Dictionary<ValueReference, Queue<int>>();

        /// <summary>
        /// Expect a delayed set to be received in the near future
        /// </summary>
        private void TrackFailedSet(ValueReference reference, int expected)
        {
            lock (_failedSets)
            {
                //init list if necessary
                if (!_failedSets.TryGetValue(reference, out Queue<int> modCounts))
                {
                    modCounts = new Queue<int>();
                    _failedSets.Add(reference, modCounts);
                }
                
                //enqueue modCount: Delayed set is expected
                modCounts.Enqueue(expected);
            }
        }

        /// <summary>
        /// Ensure the delayed set was expected
        /// </summary>
        private bool DequeueDelayedSet(ValueReference reference, int modCount)
        {
            lock (_failedSets)
            {
                //no sets for value queued
                if (!_failedSets.TryGetValue(reference, out Queue<int> modCounts)) return false;

                //all delayed sets for value were already processed
                if (!modCounts.TryPeek(out int expected)) return false;

                //Debug.Log($"Server: Delayed sets: Next: {expected}. Received: {modCount}");
                
                //another modCount is expected to be processed
                if (expected != modCount) return false;

                //delayed set will be processed
                modCounts.Dequeue();
                return true;
            }
        }
    }
}