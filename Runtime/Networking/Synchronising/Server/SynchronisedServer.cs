using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Server;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using Data_Management_for_Unity.Submodules.NetCoreServer;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Server
{
    public class SynchronisedServer : MessageServer
    {
        /// <summary>
        /// Tracks the modCount of all values of all databases
        /// </summary>
        private readonly Dictionary<ValueReference, int>
            _modCount = new Dictionary<ValueReference, int>();

        /// <summary>
        /// Increments the current modification count by one and returns it
        /// </summary>
        protected internal int IncrementModCount(ValueReference id)
        {
            lock (_modCount)
            {
                return _modCount.TryGetValue(id, out int value) ? _modCount[id] += 1 : _modCount[id] = 1;
            }
        }

        protected internal int GetModCount(ValueReference id)
        {
            lock (_modCount)
            {
                return _modCount.TryGetValue(id, out int modCount) ? modCount : 0;
            }
        }
        
        protected override TcpSession CreateSession()
        {
            return new SynchronisedSession(this);
        }
    }
}