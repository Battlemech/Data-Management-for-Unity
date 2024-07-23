using System;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Threading;
using MessagePack;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Objects
{
    [MessagePackObject]
    public class SynchronisedGameObject : DatabaseReference
    {
        /// <summary>
        /// Local instance of game object
        /// </summary>
        [IgnoreMember]
        private GameObject _gameObject;
        
        public SynchronisedGameObject(bool isPersistent = false, bool initialize = true) : base(isPersistent)
        {

        }

        public SynchronisedGameObject(string id, bool isPersistent, bool initialize) : base(id, isPersistent, true)
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

        public bool ExistsLocally()
        {
            //local game object was already retrieved
            if (_gameObject != null) return true;
            
            //try finding local instance
            _gameObject = GameObject.Find(Id);

            return _gameObject != null;
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