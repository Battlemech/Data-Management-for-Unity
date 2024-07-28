using System.Threading;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Unity.Client;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    public class TestClient : UnityClient
    {
        //create unique IDs for easier debugging
        private static int _counter;
        private readonly string _name = Interlocked.Increment(ref _counter).ToString();

        public override string ToString()
        {
            return $"{_name}:";
        }
    }
}