using System;
using NUnit.Framework.Internal;

namespace Data_Management_for_Unity.Tests.EditMode
{
    public struct TestObject
    {
        private static readonly Randomizer Random = new Randomizer();

        public readonly string Name;
        public readonly uint Id;
        public readonly Guid Id2;
        public readonly float Id3;

        public TestObject(string name)
        {
            Name = name;
            Id = Random.NextUInt();
            Id2 = Guid.NewGuid();
            Id3 = Random.NextFloat();
        }
    }
}