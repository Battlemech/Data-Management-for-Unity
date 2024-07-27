using System;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Networking.Unity.Client;
using Data_Management_for_Unity.Runtime.Threading;
using MessagePack;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Objects.GameObjects
{
    [MessagePackObject]
    public abstract class GameObjectManager : GameObjectReference
    {
        /// <summary>
        /// Local instance of the game object
        /// </summary>
        [IgnoreMember]
        private GameObject _gameObject;

        /// <summary>
        /// Manage a new gameObject. It is synchronised per default
        /// </summary>
        /// <param name="isSynchronised">Enable data synchronisation</param>
        /// <param name="isPersistent">Enable persistent data saving</param>
        /// <param name="showInNetwork">Instantly renders the managed gameObject on all clients</param>
        protected GameObjectManager(bool isSynchronised = true, bool isPersistent = true, bool showInNetwork = false) : base(isSynchronised, isPersistent)
        {
            if (showInNetwork) ShowInNetwork();
        }

        /// <summary>
        /// Manage a new or existing game object
        /// </summary>
        /// <param name="path">Path from the root of the current scene to the game object</param>
        /// <param name="isSynchronised">Enable data synchronisation</param>
        /// <param name="isPersistent">Enable persistent data saving</param>
        /// <param name="showInNetwork">Instantly renders the managed gameObject on all clients</param>
        protected GameObjectManager(string path, bool isSynchronised = true, bool isPersistent = true, bool showInNetwork = false) : base(path, isSynchronised, isPersistent)
        {
            if (showInNetwork) ShowInNetwork();
        } 
        
        public GameObject GetGameObject()
        {
            //local instance was cached
            if (_gameObject != null) return _gameObject;
            
            //try to find local instance
            _gameObject = FindInScene();
            if (_gameObject != null) return _gameObject;
            
            //create it manually
            _gameObject = new GameObject(Id);
            
            //set it up locally
            LocalConstructor(_gameObject);

            return _gameObject;
        }
        
        /// <summary>
        /// Delegate the gameObject interaction to the main thread
        /// </summary>
        public Task GetGameObject(Action<GameObject> action)
        {
            return MainThreadRunner.Delegate((() =>
            {
                action.Invoke(GetGameObject());
            }));
        }
        
        /// <summary>
        /// Delegate the component interaction to the main thread
        /// </summary>
        public Task GetComponent<T>(Action<T> action) where T : Component
        {
            return GetGameObject((o => action.Invoke(o.GetComponent<T>())));
        }

        /// <summary>
        /// Constructor is invoked whenever a gameObject is created locally for the first time
        /// </summary>
        protected abstract void LocalConstructor(GameObject gameObject);

        /// <summary>
        /// Shows the gameObject in the network
        /// </summary>
        /// <returns>True if the request could be sent to the server, otherwise false</returns>
        public bool ShowInNetwork()
        {
            //first, show it locally
            GetGameObject();
            //then, signal peers
            return UnityClient.Instance.ShowGameObject(this);
        }
    }
}