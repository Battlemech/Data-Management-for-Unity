using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Objects;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    public class TestSynchronisedObject : DatabaseReference
    {
        public ValueStorage<string> Name => GetDatabase().Get<string>(nameof(Name));

        public readonly Task SetTask;
        
        public TestSynchronisedObject(string name, int nvs, float happiness)
        {
            SetTask = Name.Set(name);
            NoValueStorage = nvs;
            Happiness = happiness;
        }

        public int NoValueStorage = 0;
        
        public float Happiness { get; protected set; }
    }
}