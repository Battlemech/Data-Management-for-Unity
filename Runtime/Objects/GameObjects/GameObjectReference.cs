using System;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Networking.Unity.Client;
using Data_Management_for_Unity.Runtime.Threading;
using MessagePack;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Data_Management_for_Unity.Runtime.Objects.GameObjects
{
    [MessagePackObject]
    public class GameObjectReference : DatabaseReference
    {
        /// <summary>
        /// Reference a new game object 
        /// </summary>
        protected GameObjectReference(bool isSynchronised = true, bool isPersistent = true) : base(isSynchronised, isPersistent)
        {
            
        }

        /// <summary>
        /// Reference a new or existing game object
        /// </summary>
        /// <param name="path">Path from the root of the current scene to the game object</param>
        /// <param name="isSynchronised">Enable data synchronisation</param>
        /// <param name="isPersistent">Enable persistent data saving</param>
        protected GameObjectReference(string path, bool isSynchronised = true, bool isPersistent = true) : base(path, isSynchronised, isPersistent)
        {
            
        }

        /// <summary>
        /// Reference a game object at the current scene
        /// </summary>
        /// <param name="toReference">Referenced game object</param>
        /// <param name="isSynchronised">Enable data synchronisation</param>
        /// <param name="isPersistent">Enable persistent data saving</param>
        public GameObjectReference(GameObject toReference, bool isSynchronised = true, bool isPersistent = true) : this(toReference.transform.GetScenePath(), isSynchronised, isPersistent)
        {
            
        }

        /// <summary>
        /// Tries to find the game object in the current scene.
        /// Can be null!
        /// </summary>
        public GameObject FindInScene()
        {
            return GameObject.Find(Id);
        }

        public override void Delete()
        {
            DestroyInNetwork();
            base.Delete();
        }

        /// <summary>
        /// Destroys the gameObject in the network
        /// </summary>
        /// <returns>True if the request could be sent to the server, otherwise false</returns>
        public bool DestroyInNetwork()
        {
            //first, destroy it locally
            OnRemoteDestroyInternal();
            //then, signal peers
            return UnityClient.Instance.DestroyGameObject(this);
        }
        
        protected internal void OnRemoteDestroyInternal()
        {
            //no need to destroy gameObject, if it doesn't exist
            GameObject gameObject = FindInScene();
            if(gameObject == null) return;
            
            //destroys the object
            OnRemoteDestroy(gameObject);
        }

        protected virtual void OnRemoteDestroy(GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
    }
}