using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Exceptions;
using DMP.Threading;
using Debug = UnityEngine.Debug;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Client
{
    public partial class MessageClient
    {
        /// <summary>
        /// Scheduler used for processing internal replies. Makes sure the replies are executed in the order in which they are received
        /// </summary>
        public readonly QueuedScheduler Scheduler = new QueuedScheduler();
        
        //todo: test main thread functionality (GameObjects)
        /// <summary>
        /// Sends a request to the server.
        /// </summary>
        /// <param name="request">Request to send</param>
        /// <param name="timeout">Amount of ms to wait before triggering a TimeOutException</param>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TReply">Type of reply</typeparam>
        /// <returns>Task returning the received reply on termination</returns>
        /// <exception cref="NotConnectedException">Client isn't connected to the server</exception>
        /// <exception cref="TimedOutException">Reply from server timed out</exception>
        public Task<TReply> SendRequest<TRequest, TReply>(TRequest request, int timeout = Options.DefaultTimeout) where TRequest : Request where TReply : Reply
        {
            ManualResetEvent receivedEvent = new ManualResetEvent(false);
                
            //allow saving received reply
            TReply reply = null;
                
            //save reply and continue execution once its received
            AddCallback<TReply>((r =>
            {
                //reply for another request
                if(r.Id != request.Id) return;
                    
                //save reply for waiting task
                reply = r;

                //signal waiting task that reply was received
                receivedEvent.Set();

                //remove callback: Reply was received
                RemoveCallbacks<TReply>(reply.Id.ToString());
            }), request.Id.ToString(), mainThread:false);
                
            //send the request
            if (!Send(request)) 
                throw new NotConnectedException();
            
            //Only delegate waiting for a reply: This ensures requests are received in the order in which they are sent
            Task<TReply> replyTask = new Task<TReply>((() =>
            {
                //wait for reply
                if (!receivedEvent.WaitOne(timeout)) 
                    throw new TimedOutException(timeout);
                
                return reply;
            }));
            
            //ensure replies are processed in the order in which they are received
            replyTask.Start(Scheduler);

            return replyTask;
        }
    }
}