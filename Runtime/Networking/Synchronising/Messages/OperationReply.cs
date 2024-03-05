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
        
        public OperationReply(OperationRequest request, int expected) : base(request)
        {
            Expected = expected;
        }

        public bool Success(int modCount) => modCount == Expected;

        public override string ToString()
        {
            return $"OperationReply, expected={Expected}";
        }
    }
}