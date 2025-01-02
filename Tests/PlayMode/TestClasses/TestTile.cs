using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Objects;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    public class TestTile : DatabaseReference
    {
        public ValueStorage<TestCoordinate> Coordinate => GetDatabase().Get<TestCoordinate>(nameof(Coordinate));
        
        public TestTile(TestCoordinate coordinate)
        {
            Coordinate.Set(coordinate);
        }
    }
}