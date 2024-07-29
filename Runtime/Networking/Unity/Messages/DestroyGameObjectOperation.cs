using Data_Management_for_Unity.Runtime.Objects;
using Data_Management_for_Unity.Runtime.Objects.GameObjects;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Networking.Unity.Messages
{
    [MessagePackObject]
    public class DestroyGameObjectOperation
    {
        [Key(0)]
        public readonly GameObjectReference Reference;
        public DestroyGameObjectOperation(GameObjectReference reference)
        {
            Reference = reference;
        }
    }
}