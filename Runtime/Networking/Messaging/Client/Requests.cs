using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Exceptions;
using Debug = UnityEngine.Debug;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Client
{
    public partial class MessageClient
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

                    //remove callback: Reply was received
                    RemoveCallbacks<TReply>(reply.Id.ToString());
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
        
        public Task SendRequest<TRequest, TReply>(TRequest request, Action<TReply> onReply, int timeout = Options.DefaultTimeout)
            where TRequest : Request where TReply : Reply
        {
            //invoke callback once reply was received
            return SendRequest<TRequest, TReply>(request, timeout).ContinueWith((task =>
            {
                //todo: test what happens when exception is thrown
                onReply.Invoke(task.Result);
            }));
        }
    }
}