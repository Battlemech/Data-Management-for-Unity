using System;
using Data_Management_for_Unity.Runtime.Networking;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using UnityEngine.UI;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    public abstract class SynchronisedOperation
    {
        /// <summary>
        /// Id of the database the operation was performed on
        /// </summary>
        public readonly string DatabaseId;

        /// <summary>
        /// Id of the value the operation was performed on
        /// </summary>
        public readonly string ValueId;
        
        /// <summary>
        /// Expected modificationCount, used to synchronise order of operations
        /// </summary>
        public int ModCount;

        protected SynchronisedOperation(string databaseId, string valueId)
        {
            DatabaseId = databaseId;
            ValueId = valueId;
        }

        /// <summary>
        /// Create a tuple containing the database and value id of value operation was performed on
        /// </summary>
        public ValueReference GetReference()
        {
            return new ValueReference(DatabaseId, ValueId);
        }
        
        /// <summary>
        /// Operation is repeated locally after synchronisation failed initially
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="type">Current type</param>
        /// <param name="resultType">Result type</param>
        /// <returns>Result value</returns>
        public abstract byte[] Repeat(byte[] value, Type type, out Type resultType);

        /// <summary>
        /// Operation is processed on remote
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="type">Current type</param>
        /// <param name="resultType">Result type</param>
        /// <returns>Result value</returns>
        public abstract byte[] OnRemote(byte[] value, Type type, out Type resultType);
    }
}