using System;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    public abstract class SynchronisedOperation
    {
        public readonly string DatabaseId;
        
        public byte[] Value;
        public Type Type;
        
        public int ModCount;

        protected SynchronisedOperation(string databaseId, byte[] value, Type type, int modCount)
        {
            DatabaseId = databaseId;
            
            Value = value;
            Type = type;
            
            ModCount = modCount;
        }

        /// <summary>
        /// Creates a SynchronisationRequest, attempting to perform the current operation in a synchronised context.
        /// Updates this classes Value and Type.
        /// </summary>
        public OperationRequest Invoke()
        {
            OnInvoke();
            return new OperationRequest(this);
        }

        /// <summary>
        /// Repeats the Operation after up-to-date data has been received from server.
        /// Updates this classes Value and Type.
        /// </summary>
        public OperationMessage Repeat()
        {
            OnRepeat();
            return new OperationMessage(this);
        }

        /// <summary>
        /// Performs the operation on the remote, returning the updated value and type.
        /// </summary>
        public abstract byte[] OnRemote(out Type type);

        /// <summary>
        /// Function is called when operation is invoked for the first time
        /// </summary>
        protected abstract void OnInvoke();

        /// <summary>
        /// Function is called when the operation is repeated after receiving up-to-date data from server.
        /// </summary>
        protected abstract void OnRepeat();
    }
}