using System;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging
{
    public abstract class Request
    {
        public readonly Guid Id = Guid.NewGuid();
    }

    public abstract class Reply
    {
        public readonly Guid Id;
        
        public Reply(Request request)
        {
            Id = request.Id;
        }
    }
}