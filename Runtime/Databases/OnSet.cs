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
        protected internal async Task OnSet<T>(string valueId, byte[] value, Type type, Action<T> onConfirmed)
        {
            await OnLocalOperation(value, type, new SynchronisedSet<T>(Id, valueId, value, type, onConfirmed));
        }

        protected internal async Task OnModify<T>(string valueId, byte[] value, Type type, ModifyDelegate<T> modify, bool isSafe, Action<T> onConfirmed)
        {
            await OnLocalOperation(value, type, new SynchronisedModify<T>(Id, valueId, value, type, modify, isSafe, onConfirmed));
        }

        protected internal async Task OnAdd<TCollection, TValue>(string valueId, byte[] collectionValue,
            Type collectionType, byte[] addedValue, Type addedType, bool isSafe, Action<TCollection> onConfirmed)
            where TCollection : ICollection<TValue>, new()
        {
            await OnLocalOperation(collectionValue, collectionType, new SynchronisedAdd<TCollection, TValue>(Id, valueId, addedValue, addedType, isSafe, onConfirmed));
        }

        protected internal async Task OnRemove<TCollection, TValue>(string valueId, byte[] collectionValue,
            Type collectionType, byte[] removedValue, Type removedType, bool isSafe, Action<TCollection> onConfirmed)
            where TCollection : ICollection<TValue>, new()
        {
            await OnLocalOperation(collectionValue, collectionType, new SynchronisedRemove<TCollection, TValue>(Id, valueId, removedValue, removedType, isSafe, onConfirmed));
        }

        protected internal async Task OnRemoveKey<TDictionary, TKey, TValue>(string valueId, byte[] collectionValue,
            Type collectionType, byte[] removedValue, Type removedType, bool isSafe, Action<TDictionary> onConfirmed)
            where TDictionary : IDictionary<TKey, TValue>, new()
        {
            await OnLocalOperation(collectionValue, collectionType, new SynchronisedKeyRemove<TDictionary, TKey, TValue>(Id, valueId, removedValue, removedType, isSafe, onConfirmed));
        }

        private async Task OnLocalOperation(byte[] value, Type type, SynchronisedOperation op)
        {
            try
            {
                //unsafe operations update local value instantly
                if (!op.IsSafeOperation())
                {
                    Invoke(op.ValueId, Serialization.Deserialize(value, type));
                }
            
                //synchronise data across multiple clients
                if (IsSynchronised) await OnLocalSynchronisedOperation(value, type, op);
                //persistent data is updated after values were confirmed by remote. If not synchronised: Update instantly
                else if (IsPersistent) await PersistentData.Save(Id, op.ValueId, value, type, op.ModCount);
            }
            catch (Exception e)
            {
                //make sure to log exception if task isn't awaited
                Debug.LogException(e);
            }
        }

        private async Task OnLocalSynchronisedOperation(byte[] value, Type type, SynchronisedOperation operation)
        {
            //make sure the client is connected //todo: synchronise
            if(!Client.IsConnected) return;
            
            //assign modCount and request operation to be executed
            OperationReply reply = await SendOperationRequest(operation);

            //process successful operation
            if (reply.Success(operation.ModCount))
                OnSuccessfulOperation(value, type, operation);
            else
                //process failed operation
                OnFailedOperation(operation, reply.Expected);
        }

        private async void OnSuccessfulOperation(byte[] value, Type type, SynchronisedOperation operation)
        {
            //if operation wasn't executed before confirmation from server
            if (operation.IsSafeOperation())
            {
                //try loading up-to-date data
                lock (_confirmed)
                {
                    //get up-to-date data, confirmed by server
                    if (_confirmed.TryGetValue(operation.ValueId, out ValueRecord data) && data.ModCount == operation.ModCount - 1)
                    {
                        value = data.Value;
                        type = data.Type;
                    }
                    else
                    {
                        value = null;
                        type = null;
                    }
                }
                
                //execute operation, invoking OnConfirmed in the process
                OnOperation(value, type, operation, true);
                return;
            }
            
            //update local data confirmed by remote
            UpdateConfirmedData(operation.ValueId, operation.ModCount, value, type);
            
            //operation was confirmed by remote
            operation.OnConfirmed(value, type);
            
            //save data persistently, if necessary
            if (IsPersistent) await PersistentData.Save(Id, operation.ValueId, value, type, operation.ModCount);
        }
        
        /// <summary>
        /// Processes a failed operation.
        /// </summary>
        /// <param name="operation">Operation which failed</param>
        /// <param name="expected">Expected modification count from server</param>
        private void OnFailedOperation(SynchronisedOperation operation, int expected)
        {
            //update modCount of operation from locally expected to remotely required
            operation.ModCount = expected;
            
            //enter critical area: Make sure no confirmed data is updated while later operation is enqueued
            lock (_confirmed)
            {
                //if required modCount was reached locally while waiting for reply: Process delayed operation instantly
                if (_confirmed.TryGetValue(operation.ValueId, out ValueRecord data) && data.ModCount == operation.ModCount - 1)
                {
                    //instantly execute operation
                    OnOperation(data.Value, data.Type, operation, true);
                }
                else
                {
                    //enqueue operation: It will be executed once up-to-date value was received
                    EnqueueDelayedOperation(operation.ValueId, operation);
                }
            }
        }

        private Task<OperationReply> SendOperationRequest(SynchronisedOperation operation)
        {
            //Make sure modCount is incremented and request is sent in one go:
            //Prevents request with higher modCount reaching server, which will think the request has expected value
            lock (Id)
            {
                operation.ModCount = IncrementModCount(operation.ValueId);
            
                return Client.SendRequest<OperationRequest, OperationReply>(new OperationRequest(operation));   
            }
        }
    }
}