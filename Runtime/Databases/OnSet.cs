using System;
using System.Threading.Tasks;
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
            //invoke local callbacks
            _callbackHandler.Invoke(id, Serialization.Deserialize(value, type));
            
            //value changed -> Increment modification count
            int modCount = IncrementModCount(id);
            
            //synchronise data across multiple clients
            if(IsSynchronised) await OnSetSynchronised(id, value, type, modCount);
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
            if(reply.Success(modCount)) return;
            
            throw new NotImplementedException();
        }
    }
}