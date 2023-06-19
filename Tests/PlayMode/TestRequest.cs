using Data_Management_for_Unity.Runtime.Networking.Messaging;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    public class TestRequest : Request
    {
        public readonly int A;
        public readonly int B;

        public TestRequest(int a, int b)
        {
            A = a;
            B = b;
        }
    }
        
    public class TestReply : Reply
    {
        public readonly int Added;
        public readonly int Multiplied;
            
        public TestReply(TestRequest request) : base(request)
        {
            Added = request.A + request.B;
            Multiplied = request.A * request.B;
        }
    }
}