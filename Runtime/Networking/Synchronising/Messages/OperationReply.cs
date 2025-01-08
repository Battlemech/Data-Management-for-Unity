using System;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    [MessagePackObject]
    public class OperationReply : Reply
    {
        /// <summary>
        /// ModCount the server expected
        /// </summary>
        [Key(1)]
        public readonly int Expected;

        /// <summary>
        /// True if the operation was successful, otherwise false
        /// </summary>
        [Key(2)]
        public readonly bool Success;
        
        public OperationReply(OperationRequest request, int expected, bool success) : base(request)
        {
            Expected = expected;
            Success = success;
        }
        
        [SerializationConstructor]
        protected OperationReply(Guid id, int expected, bool success) : base(id)
        {
            Expected = expected;
            Success = success;
        }

        public override string ToString()
        {
            return $"OperationReply, expected={Expected}";
        }
    }
}