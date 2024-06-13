using System;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Serializer;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    [MessagePackObject]
    public class OperationMessage
    {
        [Key(0)]
        public readonly SynchronisedOperation Operation;

        public OperationMessage(SynchronisedOperation operation)
        {
            Operation = operation;
        }
    }
}