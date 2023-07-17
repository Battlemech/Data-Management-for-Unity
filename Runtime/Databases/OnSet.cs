using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using Data_Management_for_Unity.Runtime.Persistence;
using Data_Management_for_Unity.Runtime.Serializer;
using DMP.Threading;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        protected internal Task OnSet(string valueId, byte[] value, Type type)
        {
            /*
             * Task factory uses this databases concurrent scheduler, making sure tasks are executed in order.
             * The task factory allows scheduling asynchronous code.
             */
            
            /*
             * Tasks executed in _factory, using QueuedScheduler, can only be executed one at a time.
             * this ensures requests being created first will be received by server first, not messing with the modCount order
             */
            return _factory.StartNew((() =>
            {
                //Value changed -> Increment modification count
                int modCount = IncrementModCount(valueId);
                
                return OnOperation(valueId, value, type,new SynchronisedSet(value, type, modCount));
            })).Unwrap();
        }

        protected internal Task OnModify<T>(string valueId, byte[] value, Type type, ModifyDelegate<T> modify)
        {
            return _factory.StartNew((() =>
            {
                //value changed -> Increment modification count
                int modCount = IncrementModCount(valueId);

                return OnOperation(valueId, value, type, new SynchronisedModify<T>(modify, modCount));
            })).Unwrap();
        }

        protected internal Task OnAdd<T, TValue>(string valueId, byte[] value, Type type) where T : ICollection<TValue>, new()
        {
            return _factory.StartNew((() =>
            {
                //value changed -> Increment modification count
                int modCount = IncrementModCount(valueId);

                return OnOperation(valueId, value, type, new SynchronisedAdd<T, TValue>(value, type, modCount));
            })).Unwrap();
        }
        
        private async Task OnOperation(string valueId, byte[] value, Type type, SynchronisedOperation op)
        {
            //invoke local callbacks
            _callbackHandler.Invoke(valueId, Serialization.Deserialize(value, type));

            //synchronise data across multiple clients
            if (IsSynchronised) await ExecuteOperation(valueId, value, type, op);
            //persistent data is updated after values were confirmed by remote. If not synchronised: Update instantly
            else if (IsPersistent) await PersistentData.Save(Id, valueId, value, type, op.ModCount);
        }

        private async Task ExecuteOperation(string valueId, byte[] value, Type type, SynchronisedOperation operation)
        {
            AccessValueReply reply = await operation.Invoke(Client, Id, valueId, value, type);
            
            //operation was successful. New data was confirmed by remote
            if (reply.Success(operation.ModCount))
            {
                //Debug.Log($"{Client} operation success, modCount={operation.ModCount}");
                
                //update local data confirmed by remote
                UpdateConfirmedData(valueId, operation.ModCount, value, type);
                
                //save data persistently, if necessary
                if (IsPersistent) await PersistentData.Save(Id, valueId, value, type, operation.ModCount);
                return;
            }

            //update modCount of operation from locally expected to remotely required
            operation.ModCount = reply.Expected;
            
            //enter critical area: Make sure no confirmed data is updated while later operation is enqueued
            lock (_confirmed)
            {
                //if required modCount was reached locally while waiting for reply: Process delayed operation instantly
                if (_confirmed.TryGetValue(valueId, out ValueRecord data) && data.ModCount == operation.ModCount - 1)
                {
                    //update data from saved value
                    value = data.Value;
                    type = data.Type;
                 
                    //Debug.Log($"{Client} operation failure, but can be executed instantly, modCount={operation.ModCount}");
                    
                    //instantly execute operation
                    ExecuteDelayedOperation(valueId, value, type, operation);
                }
                else
                {
                    //Debug.Log($"{Client} operation failure, delaying it, modCount={operation.ModCount}");
                    //enqueue operation: It will be executed once up-to-date value was received
                    EnqueueDelayedOperation(valueId, operation);
                }
            }
        }
    }
}