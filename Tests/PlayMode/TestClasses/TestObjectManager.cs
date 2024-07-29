using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Objects.GameObjects;
using UnityEngine;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    public class TestObjectManager : TypedObjectManager
    {
        public const int InitialHp = 100; 
        public ValueStorage<int> Hp => GetDatabase().Get<int>(nameof(Hp));
        
        public TestObjectManager() : base(showInNetwork: true)
        {
            
        }

        protected override List<Type> GetComponentClasses()
        {
            return new List<Type>(){typeof(TestDMPBehavior)};
        }

        protected override async void LocalConstructor(GameObject gameObject)
        {
            //add base components
            base.LocalConstructor(gameObject);
            
            //set initial hp
            await Hp.Set(InitialHp);
        }
    }
}