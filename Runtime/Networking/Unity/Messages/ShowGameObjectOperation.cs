using Data_Management_for_Unity.Runtime.Objects;
using Data_Management_for_Unity.Runtime.Objects.GameObjects;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Networking.Unity.Messages
{
    [MessagePackObject]
    public class ShowGameObjectOperation
    {
        [Key(0)]
        public readonly GameObjectManager Manager;

        public ShowGameObjectOperation(GameObjectManager manager)
        {
            Manager = manager;
        }
    }
    
    
}