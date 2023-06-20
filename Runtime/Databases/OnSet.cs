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
        }

        private void OnSetSynchronised(string id, byte[] value, Type type, int modCount)
        {
            
        }
    }
}