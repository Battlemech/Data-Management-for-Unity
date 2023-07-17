using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    public class OperationMessage
    {
        public readonly SynchronisedOperation Operation;

        public OperationMessage(SynchronisedOperation operation)
        {
            Operation = operation;
        }
    }
}