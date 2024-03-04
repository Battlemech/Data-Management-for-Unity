namespace Data_Management_for_Unity.Tests.EditMode
{
    public abstract class AbstractClass
    {
        
    }

    public class AbstractClass1 : AbstractClass
    {
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

    public class AbstractClass2 : AbstractClass
    {
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