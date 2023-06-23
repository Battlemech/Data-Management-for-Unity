using System;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Persistence;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        protected internal async Task OnSet(string id, byte[] value, Type type)
        {
            int modCount = IncrementModCount(id);
            
            //save data persistently
            if (IsPersistent) await PersistentData.Save(Id, id, value, type, modCount);
            //synchronise data across multiple clients
            if(IsSynchronised) OnSetSynchronised(id, value, type, modCount);
        }

        /// <summary>
        /// Called when the local client sets a value in this database.
        /// </summary>
        private void OnSetSynchronised(string valueId, byte[] value, Type type, int modCount)
        {
            throw new NotImplementedException();
        }
    }
}