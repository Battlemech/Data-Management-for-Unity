using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Networking.Messaging;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    public class OperationRequest : Request
    {
        public readonly SynchronisedOperation Operation;

        public OperationRequest(SynchronisedOperation operation)
        {
            Operation = operation;
        }
    }
}