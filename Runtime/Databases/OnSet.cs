using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using Data_Management_for_Unity.Runtime.Persistence;
using Data_Management_for_Unity.Runtime.Serializer;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        /// <summary>
        /// Invoked when a value is overwritten
        /// </summary>
        /// <param name="valueId">Id of value storage</param>
        /// <param name="value">Serialized value</param>
        /// <param name="type">Type of value</param>
        /// <returns></returns>
        protected internal Task OnSet(string valueId, byte[] value, Type type)
        {
            //value changed -> Increment modification count
            int modCount = IncrementModCount(valueId);

            return OnOperation(valueId, value, type,new SynchronisedSet(value, type, modCount));
        }

        /// <summary>
        /// Invoked when a value is modified
        /// </summary>
        /// <param name="valueId">Id of value storage</param>
        /// <param name="value">Serialized value</param>
        /// <param name="type">Type of value</param>
        /// <param name="modify">Delegate used to modify value</param>
        /// <returns></returns>
        protected internal Task OnModify<T>(string valueId, byte[] value, Type type, ModifyDelegate<T> modify)
        {
            //value changed -> Increment modification count
            int modCount = IncrementModCount(valueId);

            return OnOperation(valueId, value, type, new SynchronisedModify<T>(modify, modCount));
        }

        /// <summary>
        /// Invoked when a value is added to a collection
        /// </summary>
        /// <param name="valueId">Id of value storage</param>
        /// <param name="collectionValue">Serialized collection</param>
        /// <param name="collectionType">Type of collection</param>
        /// <param name="addedValue">Serialized added value</param>
        /// <param name="addedType">Type of added value</param>
        /// <returns></returns>
        protected internal Task OnAdd(string valueId, byte[] collectionValue, Type collectionType, byte[] addedValue, Type addedType)
        {
            //value changed -> Increment modification count
            int modCount = IncrementModCount(valueId);

            return OnOperation(valueId, collectionValue, collectionType, new SynchronisedAdd(addedValue, addedType, modCount));
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
                    ExecuteDelayedOperation(valueId, value, type, operation);
                else
                    //enqueue operation: It will be executed once up-to-date value was received
                    EnqueueDelayedOperation(valueId, operation);
            }
        }
    }
}