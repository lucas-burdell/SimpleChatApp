namespace SimpleChat.Core.Messages
{
    public class RoomNameMessage : BaseMessage
    {
        public required string Name { get; set; }
    }

}
