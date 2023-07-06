using System;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
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

        public override object Repeat(string databaseId, string valueId, byte[] currentValue, Type currentType)
        {
            // Overwrites current value with the one which was attempted to be set earlier
            return new SetValueMessage(databaseId, valueId, _value, _type, ModCount);
        }
    }
}