using System;
using System.Collections.Generic;
using System.Linq;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Server;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Server
{
    public partial class SynchronisedSession : MessageSession
    {
        public SynchronisedSession(SynchronisedServer server) : base(server)
        {
            //process incoming requests to perform operations
            AddCallback<OperationRequest>(((request) =>
            {
                //extract operation for easier reference
                SynchronisedOperation operation = request.Operation;
                ValueReference reference = operation.GetReference();
                
                //client is planning to change a value
                int modCount = server.IncrementModCount(reference);
                bool success = operation.IsOperationValid(modCount);

                //create reply
                OperationReply reply = new OperationReply(request, modCount, success);
                
                //if request was successful             //and client attempted instant value overwrite
                if (success && !operation.IsSafeOperation())
                {
                    //inform other clients of new value
                    server.MulticastToOthers(new OperationMessage(operation), this);
                }
                else
                {
                    //expect a OperationMessage when client received up to date data
                    TrackFailedSet(reference, modCount);
                }
                
                //send reply
                Send(reply);
            }));

            //process delayed operations
            AddCallback<OperationMessage>(((message) =>
            {
                //extract operation for easier access
                SynchronisedOperation operation = message.Operation;
                
                //if delayed set was expected
                if (DequeueDelayedSet(operation.GetReference(), operation.ModCount))
                {
                    //inform others of new value
                    server.MulticastToOthers(message, this);
                }
                else
                    //delayed set was unexpected
                    throw new InvalidOperationException("Received invalid delayed set!");
            }));
        }
    }
}