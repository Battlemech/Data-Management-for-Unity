using Data_Management_for_Unity.Runtime.Networking.Messaging;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    public class OperationReply : Reply
    {
        /// <summary>
        /// ModCount the server expected
        /// </summary>
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