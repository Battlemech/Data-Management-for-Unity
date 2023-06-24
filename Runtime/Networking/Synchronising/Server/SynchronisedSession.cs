using System;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Server;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Server
{
    public partial class SynchronisedSession : MessageSession
    {
        public SynchronisedSession(SynchronisedServer server) : base(server)
        {
            //Process incoming set requests
            AddCallback<SetValueRequest>((request =>
            {
                //client is planning to change a value
                int modCount = server.IncrementModCount(request.Reference);
                
                //create reply
                SetValueReply reply = new SetValueReply(request, modCount);

                //if request was successful:
                if (reply.Success(request.ModCount))
                {
                    //inform other clients of new value
                    server.MulticastToOthers(new SetValueMessage(request), this);
                }
                else
                {
                    TrackFailedSet(request.Reference, modCount);
                }
                
                //send reply
                Send(reply);
            }));
            
            //process delayed sets
            AddCallback<SetValueMessage>((message =>
            {
                if(DequeueDelayedSet(message.Reference, server.GetModCount(message.Reference)))
                    server.MulticastToOthers(message, this);
                else
                    throw new InvalidOperationException("Received invalid delayed set!");
            }));
        }
    }
}