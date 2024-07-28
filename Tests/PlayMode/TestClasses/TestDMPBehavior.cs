using Data_Management_for_Unity.Runtime.Objects.GameObjects;
using UnityEngine;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    public class TestDMPBehavior : DMPBehavior<TestObjectManager>
    {
        /// <summary>
        /// Locally cache synchronised hp to test callback
        /// </summary>
        public int LocalHpValue { get; private set; }
        
        protected override void OnInitialized(TestObjectManager parent)
        {
            parent.Hp.AddCallback((i =>
            {
                LocalHpValue = i;
            }));
        }
    }
}