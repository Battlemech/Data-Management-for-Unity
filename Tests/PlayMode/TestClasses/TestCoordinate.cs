using System;
using Data_Management_for_Unity.Runtime.Objects;
using MessagePack;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    [MessagePackObject]
    public class TestCoordinate
    {
        [Key(0)]
        public readonly int X;
        [Key(1)]
        public readonly int Y;

        public TestCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is TestCoordinate other && Equals(other);
        }
        
        protected bool Equals(TestCoordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}