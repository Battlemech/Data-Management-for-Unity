using System;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using MessagePack;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    [MessagePackObject]
    public class TestRequest : Request
    {
        [Key(1)]
        public readonly int A;
        
        [Key(2)]
        public readonly int B;

        public TestRequest(int a, int b)
        {
            A = a;
            B = b;
        }
        
        [SerializationConstructor]
        protected TestRequest(Guid id, int a, int b) : base(id)
        {
            A = a;
            B = b;
        }
    }
        
    [MessagePackObject]
    public class TestReply : Reply
    {
        [Key(1)]
        public readonly int Added;
        [Key(2)]
        public readonly int Multiplied;
            
        public TestReply(TestRequest request) : base(request)
        {
            Added = request.A + request.B;
            Multiplied = request.A * request.B;
        }

        [SerializationConstructor]
        protected TestReply(Guid id, int added, int multiplied) : base(id)
        {
            Added = added;
            Multiplied = multiplied;
        }
    }
}