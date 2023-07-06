using System;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    //todo: make class static??
    public abstract class SynchronisedOperation
    {
        public int ModCount;

        protected SynchronisedOperation(int modCount)
        {
            ModCount = modCount;
        }

        /// <summary>
        /// Invokes an operation, trying to perform it
        /// </summary>
        /// <param name="client"></param>
        /// <param name="databaseId">Id of database which is processing delayed operation</param>
        /// <param name="valueId">Id of value on database for which delayed operation is being processed</param>
        /// <param name="value">Current value of current value</param>
        /// <param name="type">Current type of current value</param>
        /// <returns>The reply to the initial operation</returns>
        public abstract Task<AccessValueReply> Invoke(SynchronisedClient client, string databaseId, string valueId, byte[] value, Type type);

        /// <summary>
        /// Repeats the delayed operation
        /// </summary>
        /// <param name="databaseId">Id of database which is processing delayed operation</param>
        /// <param name="valueId">Id of value on database for which delayed operation is being processed</param>
        /// <param name="currentValue">Current value of current value</param>
        /// <param name="currentType">Current type of current value</param>
        /// <returns>Object which needs to be processed locally and on remote. Example: SetValueMessage</returns>
        public abstract object Repeat(string databaseId, string valueId, byte[] currentValue, Type currentType);
    }
}