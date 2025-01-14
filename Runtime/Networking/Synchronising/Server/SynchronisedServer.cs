using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Server;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using Data_Management_for_Unity.Submodules.NetCoreServer;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Server
{
    public class SynchronisedServer : MessageServer
    {
        /// <summary>
        /// Tracks the modCount of all values of all databases
        /// </summary>
        private readonly Dictionary<ValueReference, int> _modCount = new Dictionary<ValueReference, int>();

        protected override TcpSession CreateSession()
        {
            return new SynchronisedSession(this);
        }


        /// <summary>
        /// Enqueues and validates the requested operation, changing the modCount if necessary
        /// </summary>
        /// <param name="op">Operation to validate</param>
        /// <param name="expected">Operation modCount expected by server</param>
        /// <returns>True if the request was successful, otherwise false</returns>
        protected internal bool ValidateOperation(SynchronisedOperation op, out int expected)
        {
            var reference = op.GetReference();
            
            lock (_modCount)
            {
                //get expected modCount
                expected = _modCount.GetValueOrDefault(reference, 1);
                
                //validate operation
                bool success = op.OnServerValidation(expected, out int updatedModCount);
                
                //save modCount incremented by operation
                _modCount[reference] = updatedModCount;
                
                return success;
            }
        }
    }
}