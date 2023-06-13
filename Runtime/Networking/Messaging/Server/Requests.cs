using System.Threading;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Exceptions;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Server
{
    public partial class MessageSession
    {
        public Task<TReply> SendRequest<TRequest, TReply>(TRequest request, int timeout = Options.DefaultTimeout) where TRequest : Request where TReply : Reply
        {
            Task<TReply> replyTask = new Task<TReply>((() =>
            {
                ManualResetEvent receivedEvent = new ManualResetEvent(false);
                
                //allow saving received reply
                TReply reply = null;
                
                //save reply and continue execution once its received
                AddCallback<TReply>((r =>
                {
                    //save reply for waiting task
                    reply = r;

                    //signal waiting task that reply was received
                    receivedEvent.Set();

                }), request.Id.ToString());

                //send the request
                if (!Send(request)) 
                    throw new NotConnectedException();

                //wait for reply
                if (!receivedEvent.WaitOne(timeout)) 
                    throw new TimedOutException(timeout);

                return reply;
            }));
            
            replyTask.Start();

            return replyTask;
        }
    }
}