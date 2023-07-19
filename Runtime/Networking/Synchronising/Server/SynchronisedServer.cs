using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Server;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using Data_Management_for_Unity.Submodules.NetCoreServer;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Server
{
    public partial class SynchronisedServer : MessageServer
    {
        /// <summary>
        /// Tracks the modCount of all values of all databases
        /// </summary>
        private readonly Dictionary<ValueReference, int> _modCount = new Dictionary<ValueReference, int>();

        protected virtual void Start()
        {
            //process incoming requests to perform operations
            AddCallback<OperationRequest>(((request, session) =>
            {
                //extract operation for easier reference
                SynchronisedOperation operation = request.GetOperation();
                ValueReference reference = operation.GetReference();
                
                //client is planning to change a value
                int modCount = IncrementModCount(reference);
                
                //create reply
                OperationReply reply = new OperationReply(request, modCount);
                
                //if request was successful
                if (reply.Success(operation.ModCount))
                {
                    //inform other clients of new value
                    MulticastToOthers(new OperationMessage(operation), session);
                }
                else
                {
                    //expect a OperationMessage when client received up to date data
                    session.TrackFailedSet(reference, modCount);
                }
                
                //send reply
                session.Send(reply);
            }));

            //process delayed operations
            AddCallback<OperationMessage>(((message, session) =>
            {
                //extract operation for easier access
                SynchronisedOperation operation = message.GetOperation();

                //if delayed set was expected
                if (session.DequeueDelayedSet(operation.GetReference(), operation.ModCount))
                {
                    //inform others of new value
                    MulticastToOthers(message, session);
                }
                else
                    //delayed set was unexpected
                    throw new InvalidOperationException("Received invalid delayed set!");
            }));
        }

        protected override TcpSession CreateSession()
        {
            return new SynchronisedSession(this);
        }

        /// <summary>
        /// Increments the current modification count by one and returns it
        /// </summary>
        private int IncrementModCount(ValueReference id)
        {
            lock (_modCount)
            {
                return _modCount.TryGetValue(id, out int modCount) ? _modCount[id] = modCount + 1 : _modCount[id] = 1;
            }
        }

        private int GetModCount(ValueReference id)
        {
            lock (_modCount)
            {
                return _modCount.TryGetValue(id, out int modCount) ? modCount : 0;
            }
        }
    }
}