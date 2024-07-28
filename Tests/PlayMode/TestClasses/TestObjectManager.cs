using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Objects.GameObjects;
using UnityEngine;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    public class TestObjectManager : GameObjectManager
    {
        public const int InitialHp = 100; 
        public ValueStorage<int> Hp => GetDatabase().Get<int>(nameof(Hp));
        
        public TestObjectManager() : base(showInNetwork: true)
        {
            
        }

        protected override async void LocalConstructor(GameObject gameObject)
        {
            //add listener
            gameObject.AddComponent<TestDMPBehavior>().Init(this);
            
            //set initial hp
            await Hp.Set(InitialHp);
        }
    }
}