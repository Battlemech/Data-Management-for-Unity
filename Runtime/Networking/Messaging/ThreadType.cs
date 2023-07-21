namespace Data_Management_for_Unity.Runtime.Networking.Messaging
{
    public enum ThreadType
    {
        /// <summary>
        /// The callback is executed on the receiving thread. Any callbacks of the type added to the main thread will not be triggered.
        /// </summary>
        ThreadedOnly,
        
        /// <summary>
        /// The callback is executed on Unity's main thread. If any callback of the type was added only to the receiving thread, this callback will not be triggered. 
        /// </summary>
        MainThread,
        
        /// <summary>
        /// The callback is executed on the receiving thread. Additionally, after executing the callback, all callbacks of the type on Unity's main thread will be triggered.
        /// </summary>
        Threaded
    }
}