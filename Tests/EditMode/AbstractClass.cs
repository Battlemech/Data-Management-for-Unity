using MessagePack;

namespace Data_Management_for_Unity.Tests.EditMode
{
    [Union(0, typeof(AbstractClass1))]
    [Union(1, typeof(AbstractClass2))]
    public abstract class AbstractClass
    {
        
    }

    [MessagePackObject]
    public class AbstractClass1 : AbstractClass
    {
        [Key(0)]
        public readonly string Name;

        public AbstractClass1(string name)
        {
            Name = name;
        }

        protected bool Equals(AbstractClass1 other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AbstractClass1)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }

    [MessagePackObject]
    public class AbstractClass2 : AbstractClass
    {
        [Key(0)]
        public readonly int Id;

        public AbstractClass2(int id)
        {
            Id = id;
        }

        protected bool Equals(AbstractClass2 other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AbstractClass2)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}