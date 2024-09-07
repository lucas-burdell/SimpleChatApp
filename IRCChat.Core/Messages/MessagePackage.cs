using SimpleChat.Core.Helpers;

namespace SimpleChat.Core.Messages
{
    public class MessagePackage
    {
        public MessageType MessageType { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public T GetMessage<T>() where T : BaseMessage
        {
            return BsonConvert.Deserialize<T>(Data);
        }

        public static MessagePackage MakePackage<T>(MessageType messageType, T message) where T : BaseMessage
        {
            return new MessagePackage()
            {
                MessageType = messageType,
                Data = BsonConvert.Serialize(message)
            };
        }
    }
}