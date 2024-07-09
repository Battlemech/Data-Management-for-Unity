using MessagePack;

namespace Data_Management_for_Unity.Examples.Chatroom
{
    [MessagePackObject]
    public class ChatMessage
    {
        [Key(0)]
        public readonly string Name;
        [Key(1)]
        public readonly string Message;

        public ChatMessage(string name, string message)
        {
            Name = name;
            Message = message;
        }
    }
}