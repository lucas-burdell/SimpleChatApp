namespace SimpleChat.Core
{
    public class ChatLog
    {
        public static string SYSTEM_NAME = "System";

        public DateTime DateTime { get; set; } = DateTime.Now;
        public required string Name { get; set; }
        public required string Message { get; set; }
        public required ChatLogType Type { get; set; }

        public static ChatLog CreateSystemChatLog(string message)
        {
            return new ChatLog() { Message = message, Name = SYSTEM_NAME, Type = ChatLogType.System };
        }
    }
}
