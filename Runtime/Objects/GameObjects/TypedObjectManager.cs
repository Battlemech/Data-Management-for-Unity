using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Objects.GameObjects
{
    public abstract class TypedObjectManager : GameObjectManager
    {
        /// <summary>
        /// Manage a new gameObject. It is synchronised per default
        /// </summary>
        /// <param name="isSynchronised">Enable data synchronisation</param>
        /// <param name="isPersistent">Enable persistent data saving</param>
        /// <param name="showInNetwork">Instantly renders the managed gameObject on all clients</param>
        protected TypedObjectManager(bool isSynchronised = true, bool isPersistent = true, bool showInNetwork = false) : base(isSynchronised, isPersistent, showInNetwork)
        {
            
        }
        
        /// <summary>
        /// Manage a new or existing game object
        /// </summary>
        /// <param name="path">Path from the root of the current scene to the game object</param>
        /// <param name="isSynchronised">Enable data synchronisation</param>
        /// <param name="isPersistent">Enable persistent data saving</param>
        /// <param name="showInNetwork">Instantly renders the managed gameObject on all clients</param>
        protected TypedObjectManager(string path, bool isSynchronised = true, bool isPersistent = true, bool showInNetwork = false) : base(path, isSynchronised, isPersistent, showInNetwork)
        {
            
        }
        
        
        /// <summary>
        /// Gets all components of the manager which are added and initialized automatically.
        /// Classes must be of type DMPBehavior&lt;MANAGER_CLASS&gt;
        /// </summary>
        protected abstract List<Type> GetComponentClasses();

        protected override void LocalConstructor(GameObject gameObject)
        {
            foreach (var componentClass in GetComponentClasses())
            {
                if (!typeof(DMPBehavior).IsAssignableFrom(componentClass))
                    throw new ArgumentException($"Can't initialize component of type {componentClass}, since its no DMPBehavior!");
                
                //add and initialize component
                var component = (DMPBehavior) gameObject.AddComponent(componentClass);
                component.UnsafeInit(this);
            }
        }
    }
}