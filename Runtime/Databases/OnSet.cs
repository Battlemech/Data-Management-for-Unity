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
        protected internal async Task OnSet(string valueId, byte[] value, Type type)
        {
            await OnLocalOperation(value, type, new SynchronisedSet(Id, valueId, value, type));
        }

        protected internal async Task OnModify<T>(string valueId, byte[] value, Type type, ModifyDelegate<T> modify)
        {
            await OnLocalOperation(value, type, new SynchronisedModify<T>(Id, valueId, value, type, modify));
        }

        protected internal async Task OnAdd<TCollection, TValue>(string valueId, byte[] collectionValue,
            Type collectionType, byte[] addedValue, Type addedType)
            where TCollection : ICollection<TValue>, new()
        {
            await OnLocalOperation(collectionValue, collectionType, new SynchronisedAdd<TCollection, TValue>(Id, valueId, addedValue, addedType));
        }

        protected internal async Task OnRemove<TCollection, TValue>(string valueId, byte[] collectionValue,
            Type collectionType, byte[] removedValue, Type removedType)
            where TCollection : ICollection<TValue>, new()
        {
            await OnLocalOperation(collectionValue, collectionType, new SynchronisedRemove<TCollection, TValue>(Id, valueId, removedValue, removedType));
        }

        protected internal async Task OnRemoveKey<TDictionary, TKey, TValue>(string valueId, byte[] collectionValue,
            Type collectionType, byte[] removedValue, Type removedType)
            where TDictionary : IDictionary<TKey, TValue>, new()
        {
            await OnLocalOperation(collectionValue, collectionType, new SynchronisedKeyRemove<TDictionary, TKey, TValue>(Id, valueId, removedValue, removedType));
        }

        private async Task OnLocalOperation(byte[] value, Type type, SynchronisedOperation op)
        {
            //invoke local callbacks
            _callbackHandler.Invoke(op.ValueId, Serialization.Deserialize(value, type));

            //synchronise data across multiple clients
            if (IsSynchronised) await OnLocalSynchronisedOperation(value, type, op);
            //persistent data is updated after values were confirmed by remote. If not synchronised: Update instantly
            else if (IsPersistent) await PersistentData.Save(Id, op.ValueId, value, type, op.ModCount);
        }

        private async Task OnLocalSynchronisedOperation(byte[] value, Type type, SynchronisedOperation operation)
        {
            //assign modCount and request operation to be executed
            OperationReply reply = await SendOperationRequest(operation);

            //operation was successful. New data was confirmed by remote
            if (reply.Success(operation.ModCount))
            {
                //update local data confirmed by remote
                UpdateConfirmedData(operation.ValueId, operation.ModCount, value, type);
                
                //save data persistently, if necessary
                if (IsPersistent) await PersistentData.Save(Id, operation.ValueId, value, type, operation.ModCount);
                return;
            }

            //update modCount of operation from locally expected to remotely required
            operation.ModCount = reply.Expected;
            
            //enter critical area: Make sure no confirmed data is updated while later operation is enqueued
            lock (_confirmed)
            {
                //if required modCount was reached locally while waiting for reply: Process delayed operation instantly
                if (_confirmed.TryGetValue(operation.ValueId, out ValueRecord data) && data.ModCount == operation.ModCount - 1)
                {
                    //update data from saved value
                    value = data.Value;
                    type = data.Type;
                 
                    //instantly execute operation
                    OnOperation(value, type, operation, true);
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