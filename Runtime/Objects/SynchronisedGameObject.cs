using System;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Threading;
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
        
        public SynchronisedGameObject(bool isPersistent = true) : base(isPersistent)
        {
            
        }
        
        public SynchronisedGameObject(string id, bool isPersistent=true) : base(id, isPersistent)
        {
            
        }

        /// <summary>
        /// Retrieves this objects instance of a gameObjects, or creates it if necessary
        /// </summary>
        /// <remarks>Must be executed on Unity's main thread!</remarks>
        public GameObject GetGameObject()
        {
            //local instance exists
            if (_gameObject != null) return _gameObject;

            //try finding local instance
            _gameObject = GameObject.Find(Id);
            if (_gameObject != null) return _gameObject;
                
            //create gameObject
            _gameObject = new GameObject(Id);
                
            //invoke "constructor"
            OnCreated(_gameObject);

            return _gameObject;
        }

        /// <summary>
        /// Interacts with this objects instance of a game object on Unity's main thread
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Task GetGameObject(Action<GameObject> action)
        {
            return MainThreadRunner.Delegate((() =>
            {
                action.Invoke(GetGameObject());
            }));
        }

        public Task GetComponent<T>(Action<T> action) where T : Component
        {
            return GetGameObject((o => action.Invoke(o.GetComponent<T>())));
        }

        protected virtual void OnCreated(GameObject gameObject)
        {
            
        }
    }
}