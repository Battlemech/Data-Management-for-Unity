using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.DelayedOperations;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using Data_Management_for_Unity.Runtime.Persistence;
using Data_Management_for_Unity.Runtime.Serializer;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        protected internal async Task OnSet(string id, byte[] value, Type type)
        {
            //value changed -> Increment modification count
            int modCount = IncrementModCount(id);
            
            //invoke local callbacks
            _callbackHandler.Invoke(id, Serialization.Deserialize(value, type));

            //synchronise data across multiple clients
            if (IsSynchronised) await OnSetSynchronised(id, value, type, modCount);
            //save data persistently
            if (IsPersistent) await PersistentData.Save(Id, id, value, type, modCount);
        }

        /// <summary>
        /// Called when the local client sets a value in this database.
        /// </summary>
        private async Task OnSetSynchronised(string valueId, byte[] value, Type type, int modCount)
        {
            //create request which can be sent to server
            SetValueRequest request = new SetValueRequest(Id, valueId, value, type, modCount);

            //wait for reply
            SetValueReply reply = await Client.SendRequest<SetValueRequest, SetValueReply>(request);

            //request was successful. No further action needed
            if (reply.Success(modCount)) return;
            
            //enter critical area: Make sure no confirmed data is updated while later operation is enqueued
            lock (_confirmed)
            {
                //if required modCount was reached locally while waiting for reply: Process delayed set instantly
                if (_confirmed.TryGetValue(valueId, out ValueRecord data) && data.ModCount == reply.Expected - 1)
                {
                    //inform peers of new value
                    Client.Send(new SetValueMessage(Id, valueId, value, type, reply.Expected));
                
                    //process confirmed set locally
                    OnRemoteSet(valueId, value, type, reply.Expected);
                    return;
                }
                
                //enqueue operation: It will be executed once up-to-date value was received
                EnqueueDelayedOperation(valueId, new DelayedSet(value, type, reply.Expected));
            }
        }
    }
}