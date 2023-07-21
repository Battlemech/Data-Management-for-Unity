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
        //todo: test main thread functionality (GameObjects)
        /// <summary>
        /// Sends a request to the server.
        /// </summary>
        /// <param name="request">Request to send</param>
        /// <param name="timeout">Amount of ms to wait before triggering a TimeOutException</param>
        /// <param name="threadedOnly">True if only threadedCallbacks will be triggered when receiving this reply</param>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TReply">Type of reply</typeparam>
        /// <returns>Task returning the received reply on termination</returns>
        /// <exception cref="NotConnectedException">Client isn't connected to the server</exception>
        /// <exception cref="TimedOutException">Reply from server timed out</exception>
        public Task<TReply> SendRequest<TRequest, TReply>(TRequest request, int timeout = Options.DefaultTimeout, bool threadedOnly = true) where TRequest : Request where TReply : Reply
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
            }), request.Id.ToString(), type: threadedOnly ? ThreadType.ThreadedOnly : ThreadType.Threaded);
                
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
            }), TaskCreationOptions.LongRunning);
            
            replyTask.Start();

            return replyTask;
        }
    }
}