using System;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
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

        public override object Repeat(string databaseId, string valueId, byte[] currentValue, Type currentType)
        {
            //repeat operation with up-to-date value, overwriting old value and type
            currentValue = _modify.InvokeSafe(currentValue, currentType, out currentType);

            return new SetValueMessage(databaseId, valueId, currentValue, currentType, ModCount);
        }
    }
}