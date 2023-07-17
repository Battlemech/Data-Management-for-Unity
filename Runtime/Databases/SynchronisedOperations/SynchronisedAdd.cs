using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    public class SynchronisedAdd<T, TValue> : SynchronisedOperation where T : ICollection<TValue>, new()
    {
        //value added to collection
        private readonly byte[] _value;
        //type of value added to collection
        private readonly Type _type;
        
        public SynchronisedAdd(byte[] value, Type type, int modCount) : base(modCount)
        {
            _value = value;
            _type = type;
        }

        public override async Task<AccessValueReply> Invoke(SynchronisedClient client, string databaseId, string valueId, byte[] value, Type type)
        {
            AddValueRequest request = new AddValueRequest(databaseId, valueId, value, type, ModCount);

            return await client.SendRequest<AddValueRequest, AccessValueReply>(request);
        }

        public override object Repeat(string databaseId, string valueId, byte[] value, Type type)
        {
            return new AddValueMessage(databaseId, valueId, _value, _type, ModCount);
        }
    }
}