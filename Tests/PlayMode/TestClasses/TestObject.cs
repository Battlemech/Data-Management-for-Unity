using System;
using MessagePack;
using NUnit.Framework.Internal;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    [MessagePackObject]
    public struct TestObject
    {
        private static readonly Randomizer Random = new Randomizer();

        [Key(0)]
        public readonly string Name;
        
        [Key(1)]
        public readonly uint Id;
        
        [Key(2)]
        public readonly Guid Id2;
        
        [Key(3)]
        public readonly float Id3;

        public TestObject(string name)
        {
            Name = name;
            Id = Random.NextUInt();
            Id2 = Guid.NewGuid();
            Id3 = Random.NextFloat();
        }
        
        [SerializationConstructor]
        public TestObject(string name, uint id, Guid id2, float id3)
        {
            Name = name;
            Id = id;
            Id2 = id2;
            Id3 = id3;
        }
    }
}