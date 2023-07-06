using System;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;

namespace Data_Management_for_Unity.Runtime.Databases.DelayedOperations
{
    public abstract class SynchronisedOperation
    {
        public int ModCount;

        protected SynchronisedOperation(int modCount)
        {
            ModCount = modCount;
        }

        /// <summary>
        /// Invokes an operation, trying to perform it
        /// </summary>
        /// <param name="client"></param>
        /// <param name="databaseId">Id of database which is processing delayed operation</param>
        /// <param name="valueId">Id of value on database for which delayed operation is being processed</param>
        /// <param name="value">Current value of current value</param>
        /// <param name="type">Current type of current value</param>
        /// <returns>The reply to the initial operation</returns>
        public abstract Task<AccessValueReply> Invoke(SynchronisedClient client, string databaseId, string valueId, byte[] value, Type type);

        /// <summary>
        /// Repeats the delayed operation
        /// </summary>
        /// <param name="databaseId">Id of database which is processing delayed operation</param>
        /// <param name="valueId">Id of value on database for which delayed operation is being processed</param>
        /// <param name="value">Current value of current value</param>
        /// <param name="type">Current type of current value</param>
        /// <returns>Object which needs to be processed locally and on remote. Example: SetValueMessage</returns>
        public abstract object Repeat(string databaseId, string valueId, byte[] value, Type type);
    }
    
    public class SynchronisedSet : SynchronisedOperation
    {
        private readonly byte[] _value;
        private readonly Type _type;
        
        public SynchronisedSet(byte[] value, Type type, int modCount) : base(modCount)
        {
            _value = value;
            _type = type;
        }

        public override async Task<AccessValueReply> Invoke(SynchronisedClient client, string databaseId, string valueId, byte[] value, Type type)
        {
            //create request which can be sent to server
            SetValueRequest request = new SetValueRequest(databaseId, valueId, value, type, ModCount);

            //wait for reply
            return await client.SendRequest<SetValueRequest, AccessValueReply>(request);
        }

        public override object Repeat(string databaseId, string valueId, byte[] value, Type type)
        {
            // Overwrites current value with the one which was attempted to be set earlier
            return new SetValueMessage(databaseId, valueId, _value, _type, ModCount);
        }
    }
    
    public class SynchronisedModify<T> : SynchronisedOperation
    {
        private readonly ModifyDelegate<T> _modify;

        public SynchronisedModify(ModifyDelegate<T> modify, int modCount) : base(modCount)
        {
            _modify = modify;
        }

        public override async Task<AccessValueReply> Invoke(SynchronisedClient client, string databaseId, string valueId, byte[] value, Type type)
        {
            //create request which can be sent to server
            SetValueRequest request = new SetValueRequest(databaseId, valueId, value, type, ModCount);

            //wait for reply
            return await client.SendRequest<SetValueRequest, AccessValueReply>(request);
        }

        public override object Repeat(string databaseId, string valueId, byte[] value, Type type)
        {
            //repeat operation with up-to-date value, overwriting old value and type
            value = _modify.InvokeSafe(value, type, out type);

            return new SetValueMessage(databaseId, valueId, value, type, ModCount);
        }
    }
}