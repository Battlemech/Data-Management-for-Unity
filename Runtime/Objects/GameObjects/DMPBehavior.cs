using System;
using Data_Management_for_Unity.Runtime.Databases;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Objects.GameObjects
{
    public abstract class DMPBehavior : MonoBehaviour
    {
        /// <summary>
        /// Data synchronisation is enabled when the component is initialized
        /// </summary>
        public bool initialSynchronisation = true;
        
        /// <summary>
        /// Data persistence is enabled when the component is initialized
        /// </summary>
        public bool initialPersistence = true;
        
        /// <summary>
        /// Delete all data of the DMPBehavior when the gameObject it is attached to is destroyed
        /// </summary>
        public bool deleteChildOnDestroy = false;
        
        //internally created ref to local database
        private DatabaseReference _dbRef;

        protected virtual void Awake()
        {
            //unique identifier: Path to gameObject in scene + name of component class
            _dbRef ??= new DatabaseReference($"{transform.GetScenePath()}/{GetType().Name}", initialSynchronisation, initialPersistence);
        }

        protected virtual void OnDestroy()
        {
            //delete database, if specified
            if(deleteChildOnDestroy) _dbRef.Delete();
        }

        /// <summary>
        /// Retrieves the database of the DMPBehavior, allowing to synchronise and persistently save data, assuming the options have been enabled
        /// </summary>
        public Database GetDatabase() => _dbRef.GetDatabase();
    }

    public abstract class DMPBehavior<T> : DMPBehavior 
        where T : GameObjectManager
    {
        /// <summary>
        /// Delete the associated parent script once the component is destroyed
        /// </summary>
        public bool deleteParentOnDestroy = false;
        
        private T _parent;
        
        /// <summary>
        /// Initializes the behavior given a parent object
        /// </summary>
        /// <param name="parent"></param>
        public void Init(T parent)
        {
            _parent = parent;
            OnInitialized(parent);
        }
        
        protected virtual void OnInitialized(T parent){}
        
        public T GetParent()
        {
            if (_parent == null)
                throw new InvalidOperationException("Component must be initialized before parent can be accessed!");
            return _parent;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(deleteParentOnDestroy) GetParent().Delete();
        }
    }

    public static class DMPBehaviorUtility
    {
        public static string GetScenePath(this Transform transform)
        {
            Transform parent = transform.parent;
            //recursively get names of parent objects
            return parent == null ? transform.name : $"{GetScenePath(parent)}/{transform.name}";
        }
    }
}