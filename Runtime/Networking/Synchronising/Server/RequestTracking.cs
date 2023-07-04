using System;
using System.Collections.Generic;
using System.Linq;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Server;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Server
{
    public class SynchronisedSession : MessageSession
    {
        public SynchronisedSession(MessageServer messageServer) : base(messageServer)
        {
            
        }
        
        private readonly Dictionary<ValueReference, Queue<int>> _failedSets =
            new Dictionary<ValueReference, Queue<int>>();

        protected internal void TrackFailedSet(ValueReference reference, int expected)
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

        protected internal bool DequeueDelayedSet(ValueReference reference, int modCount)
        {
            lock (_failedSets)
            {
                //no sets for value queued
                if (!_failedSets.TryGetValue(reference, out Queue<int> modCounts)) return false;

                //all delayed sets for value were already processed
                if (!modCounts.TryPeek(out int expected)) return false;

                //another modCount is expected to be processed
                if (expected != modCount) return false;

                //delayed set will be processed
                modCounts.Dequeue();
                return true;
            }
        }
    }
}