using System;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging
{
    [MessagePackObject]
    public abstract class Request
    {
        [Key(0)]
        public readonly Guid Id = Guid.NewGuid();
    }

    [MessagePackObject]
    public abstract class Reply
    {
        [Key(0)]
        public readonly Guid Id;
        
        public Reply(Request request)
        {
            Id = request.Id;
        }
    }
}