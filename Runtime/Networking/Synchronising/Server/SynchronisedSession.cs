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
                
                //client is planning to change a value
                bool success = server.ValidateOperation(operation, out int modCount);
                
                Debug.Log($"{this}: Expected: {modCount}, Received: {operation.ModCount}. Success: {success}");

                //create reply
                OperationReply reply = new OperationReply(request, modCount, success);
                
                //if request was successful             //and client attempted instant value overwrite
                if (success && !operation.IsSafeOperation())
                {
                    //inform other clients of new value
                    server.MulticastToOthers(new OperationMessage(operation), this);
                }
                else if(!operation.DiscardOnFailure())
                {
                    Debug.Log($"{this}: Delaying operation with modCount {operation.ModCount} -> {modCount}");
                    
                    //expect a OperationMessage when client received up to date data
                    TrackDelayedOperation(operation.GetReference(), modCount);
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
                if (DequeueDelayedOperation(operation.GetReference(), operation.ModCount))
                {
                    Debug.Log(this + ": Processing delayed operation: " + operation.ModCount);
                    
                    //inform others of new value
                    server.MulticastToOthers(message, this);
                }
                else
                    //delayed set was unexpected
                    throw new InvalidOperationException(this + ": Received invalid delayed set! modCount: " + operation.ModCount);
            }));
        }
    }
}