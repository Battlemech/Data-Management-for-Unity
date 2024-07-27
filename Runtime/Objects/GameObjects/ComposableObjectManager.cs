using System;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using MessagePack;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Objects.GameObjects
{
    [MessagePackObject]
    public class ComposableObjectManager : GameObjectManager
    {
        protected ValueStorage<Type[]> DmpBehaviorTypes => GetDatabase().Get<Type[]>(nameof(DmpBehaviorTypes));

        /// <summary>
        /// Manage a new gameObject. It is synchronised per default
        /// </summary>
        /// <param name="isPersistent">Saves the data persistently</param>
        /// <param name="showInNetwork">Instantly renders the managed gameObject on all clients</param>
        /// <param name="componentTypes">List of DMPBehavior types added to the object per default</param>
        protected ComposableObjectManager(bool isPersistent = true, bool showInNetwork = false, params Type[] componentTypes) : base(isPersistent, showInNetwork)
        {
            ValidateTypes(componentTypes);
            DmpBehaviorTypes.Set(componentTypes);
        }

        /// <summary>
        /// Manage a new or existing game object
        /// </summary>
        /// <param name="path">Path from the root of the current scene to the game object</param>
        /// <param name="isPersistent">Enable persistent data saving</param>
        /// <param name="isSynchronised">Enable data synchronisation</param>
        /// <param name="showInNetwork">Instantly renders the managed gameObject on all clients</param>
        /// <param name="componentTypes">List of DMPBehavior types added to the object per default</param>
        protected ComposableObjectManager(string path, bool isPersistent, bool isSynchronised, bool showInNetwork = false, params Type[] componentTypes) : base(path, isSynchronised, isPersistent, showInNetwork)
        {
            ValidateTypes(componentTypes);
            DmpBehaviorTypes.Set(componentTypes);
        }

        private void ValidateTypes(Type[] types)
        {
            foreach (var type in types)
            {
                //ensure the type is assignable to DMPBehavior
                if (!typeof(DMPBehavior).IsAssignableFrom(type)) 
                    throw new ArgumentException($"Type {type} is not assignable to DMPBehavior");
                
                //ensure the type is a generic DMPBehavior
                var genericTypes = type.GetGenericArguments();
                if(genericTypes.Length != 1) throw new ArgumentException($"Type {type} is not a generic DMPBehavior");
                
                //ensure generic type is assignable to this
                //todo: does this also work for interfaces?
                if (!GetType().IsAssignableFrom(genericTypes[0]))
                    throw new ArgumentException($"Type {genericTypes[0]} is not assignable to {GetType().FullName}");
            }
        }

        protected override void LocalConstructor(GameObject gameObject)
        {
            DmpBehaviorTypes.BlockingGet((types =>
            {
                //add all component types specified in _dmpBehaviourTypes
                foreach (var type in types)
                {
                    //add component
                    var component = gameObject.AddComponent(type);
                
                    //call the init function
                    var method = type.GetMethod("Init");
                    if (method == null) throw new ArgumentException($"Type {type} can't be a generic DMPBehavior: It lacks the Init method");
                    method.Invoke(component, new object[] {this});
                }
            }));
        }
    }
}