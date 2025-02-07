using System;
using System.Collections;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Objects;
using Data_Management_for_Unity.Runtime.Threading;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        /// <summary>
        /// Shares the databases values in the network
        /// </summary>
        /// <param name="recursive">Also share all databases referenced in the database itself</param>
        public void ShareInNetwork(bool recursive)
        {
            //synchronise used values
            lock (_values)
            {
                foreach (var storage in _values.Values)
                {
                    //share value
                    var id = storage.Id;
                    var value = storage.Serialize(out Type type);
                    ShareInNetwork(id, value, type, GetModCount(id));
                    
                    //trigger cascading value sharing
                    if(!recursive) continue;
                    RecursiveShareInNetwork(storage.UnsafeGet());
                }
            }

            //synchronise values which could not be loaded
            lock (_toLoad)
            {
                foreach (var persistentObject in _toLoad.Values)
                {
                    //share value
                    ShareInNetwork(persistentObject.ValueId, persistentObject.Value, persistentObject.Type, persistentObject.ModCount);
                    
                    //trigger cascading value sharing
                    if(!recursive) continue;
                    RecursiveShareInNetwork(persistentObject.Deserialize());
                }
            }
        }

        private void RecursiveShareInNetwork(object obj)
        {
            switch (obj)
            {
                //share simple references
                case DatabaseReference ref1:
                    ref1.ShareInNetwork(true);
                    return;
                
                //share lists
                case ICollection<DatabaseReference> collection:
                {
                    foreach (var ref2 in collection)
                    {
                        ref2.ShareInNetwork(true);
                    }
                    return;
                }
                
                //share dicts
                case IDictionary dict:
                {
                    foreach (var entry in dict.Keys)
                    {
                        RecursiveShareInNetwork(entry);
                    }
                    foreach (var value in dict.Values)
                    {
                        RecursiveShareInNetwork(value);
                    }
                    return;
                }
            }
        }

        private void ShareInNetwork(string valueId, byte[] value, Type type, int modCount)
        {
            OnLocalOperation(value, type, new SynchronisedInform(Id, valueId, value, type, modCount)).LogOnFailure();
        }
    }
}