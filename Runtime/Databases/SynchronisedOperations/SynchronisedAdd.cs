using System;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    public class SynchronisedAdd : SynchronisedOperation
    {
        private readonly byte[] _addedValue;
        private readonly Type _addedType;
        
        public SynchronisedAdd(byte[] addedValue, Type addedType, int modCount) : base(modCount)
        {
            _addedValue = addedValue;
            _addedType = addedType;
        }

        public override async Task<AccessValueReply> Invoke(SynchronisedClient client, string databaseId, string valueId, byte[] value, Type type)
        {
            AddValueRequest request = new AddValueRequest(databaseId, valueId, _addedValue, _addedType, ModCount);

            return await client.SendRequest<AddValueRequest, AccessValueReply>(request);
        }

        public override object Repeat(string databaseId, string valueId, byte[] currentValue, Type currentType)
        {
            return new AddValueMessage(databaseId, valueId, _addedValue, _addedType, ModCount);
        }
    }
}