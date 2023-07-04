using System;
using System.Collections.Generic;
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
            //process incoming requests
            AddCallback<SetValueRequest>(((request, session) =>
            {
                //client is planning to change a value
                int modCount = IncrementModCount(request.Reference);
                
                //create reply
                SetValueReply reply = new SetValueReply(request, modCount);
                
                //if request was successful:
                if (reply.Success(request.ModCount))
                {
                    //inform other clients of new value
                    MulticastToOthers(new SetValueMessage(request), session);
                }
                else
                {
                    session.TrackFailedSet(request.Reference, modCount);
                }

                //send reply
                session.Send(reply);
            }));

            //process delayed sets
            AddCallback<SetValueMessage>(((message, session) =>
            {
                //if delayed set was expected
                if (session.DequeueDelayedSet(message.Reference, message.ModCount))
                    //inform others of new value
                    MulticastToOthers(message, session);
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