namespace SimpleChat.Core.Messages
{
    public class ChatMessage : BaseMessage
    {
        public required string Name { get; set; }
        public required string Message { get; set; }
    }
}
