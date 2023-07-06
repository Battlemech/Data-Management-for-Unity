using System;
using Data_Management_for_Unity.Runtime.Networking.Messaging;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    public class AddValueRequest : SetValueRequest
    {
        public AddValueRequest(string databaseId, string valueId, byte[] value, Type type, int modCount) : base(databaseId, valueId, value, type, modCount)
        {
            
        }

        public override SetValueMessage ToMessage()
        {
            return new AddValueMessage(this);
        }
    }
    
    public class AddValueMessage : SetValueMessage
    {
        public AddValueMessage(string databaseId, string valueId, byte[] value, Type type, int modCount) : base(databaseId, valueId, value, type, modCount)
        {
        }

        public AddValueMessage(SetValueRequest request) : base(request)
        {
        }
    }
}