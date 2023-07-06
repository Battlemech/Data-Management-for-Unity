using Data_Management_for_Unity.Runtime.Networking.Messaging;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    public abstract class ValueRequest : Request
    {
        public abstract SetValueMessage ToMessage();
    }
}