using System;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Exceptions
{
    public class TimedOutException : Exception
    {
        public TimedOutException(int timeout) : base($"Received no reply within {timeout} ms!")
        {
            
        }
    }
}