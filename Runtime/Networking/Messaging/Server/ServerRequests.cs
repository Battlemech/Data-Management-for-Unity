using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.TextCore.Text;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Server
{
    public partial class MessageServer
    {
        public Task<TReply[]> SendRequests<TRequest, TReply>(TRequest request, int timeout = Options.DefaultTimeout)
            where TRequest : Request where TReply : Reply
        {
            //get all sessions, casting them to MessageSession to allow accessing request function
            return Task.WhenAll(Sessions.Values.Cast<MessageSession>()
                //send the request from all sessions
                .Select((session => session.SendRequest<TRequest, TReply>(request, timeout))));
        }
    }
}