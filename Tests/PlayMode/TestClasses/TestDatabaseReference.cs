using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Objects;
using MessagePack;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    [MessagePackObject]
    public class TestDatabaseReference : DatabaseReference
    {
        public ValueStorage<string> Name => GetDatabase().Get<string>(nameof(Name));
        
        public readonly Task SetTask;
        
        public TestDatabaseReference(string name, int nvs, float happiness)
        {
            SetTask = Name.Set(name);
            NoValueStorage = nvs;
            Happiness = happiness;
        }
        
        public int NoValueStorage = 0;
        
        public float Happiness { get; protected set; }
    }
}