using System;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Runtime.Serializer;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    [MessagePackObject]
    public class OperationRequest : Request
    {
        //serialize operation since it is an abstract class
        [Key(1)]
        public readonly SynchronisedOperation Operation;

        public OperationRequest(SynchronisedOperation operation)
        {
            Operation = operation;
        }
        
        [SerializationConstructor]
        protected OperationRequest(Guid id, SynchronisedOperation operation) : base(id)
        {
            Operation = operation;
        }
    }
}