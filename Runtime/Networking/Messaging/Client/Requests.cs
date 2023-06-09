using System;
using System.Threading;
using System.Threading.Tasks;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Client
{
    public partial class MessageClient
    {
        public Task<TReply> SendRequest<TRequest, TReply>(TRequest request) where TRequest : Request where TReply : Reply
        {
            throw new NotImplementedException();
        }
    }
}