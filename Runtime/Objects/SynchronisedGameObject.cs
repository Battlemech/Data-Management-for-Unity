using DMP.Utility;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Objects
{
    public class SynchronisedGameObject : SynchronisedObject
    {
        /// <summary>
        /// Local instance of game object
        /// </summary>
        [PreventSerialization]
        private GameObject _gameObject;
        
        public SynchronisedGameObject(string id, bool isPersistent) : base(id, isPersistent)
        {
            
        }
    }
}