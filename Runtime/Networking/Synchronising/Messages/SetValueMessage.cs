namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    public class SetValueMessage
    {
        public readonly string DatabaseId;
        public readonly string ValueId;
        public readonly object Value;
        public readonly uint ModCount;
    }
}