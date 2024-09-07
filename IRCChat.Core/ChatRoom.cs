
using SimpleChat.Core.Messages;

namespace SimpleChat.Core
{
    public class ChatRoom // room represents a connection to a server
    {
        public string UserName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public List<ChatLog> Log { get; set; } = new List<ChatLog>();
        public event EventHandler<ChatLog>? LogAdded;

        private readonly MessageEventMultiplexer _multiplexer;

        public ChatRoom(MessageEventMultiplexer multiplexer, string userName)
        {
            this._multiplexer = multiplexer;
            UserName = userName;
            SetupMessageActions();
            SendMessage(new ConnectedMessage() { Name = UserName });
        }

        public void SetupMessageActions()
        {
            _multiplexer.OnRoomNameMessage += OnRoomNameMessage;
            _multiplexer.OnChatMessage += OnChatMessage;
            _multiplexer.OnConnectedMessage += OnConnectedMessage;
        }

        private void OnRoomNameMessage(object? sender, RoomNameMessage message)
        {
            if (RoomName != "")
            {
                AddToLog(ChatLog.CreateSystemChatLog($"Room name changed to {message.Name}"));
            }
            RoomName = message.Name;
        }

        private void OnChatMessage(object? sender, ChatMessage message)
        {
            AddToLog(new ChatLog() { Name = message.Name, Message = message.Message, Type = ChatLogType.Chat });
        }

        private void OnConnectedMessage(object? sender, ConnectedMessage message)
        {
            AddToLog(ChatLog.CreateSystemChatLog($"{message.Name} has connected"));
            if (sender is ChatTransceiver chatTransceiver && _multiplexer.IsHost)
            {
                _ = chatTransceiver.TrySendMessageAsync(MessagePackage.MakePackage(MessageType.RoomName, new RoomNameMessage() { Name = RoomName }));
            }
        }

        private void AddToLog(ChatLog log)
        {
            Log.Add(log);
            LogAdded?.Invoke(this, log);
        }

        public void SendMessage(ConnectedMessage message)
        {
            _multiplexer.SendMessage(MessagePackage.MakePackage(MessageType.Connected, message));
            AddToLog(ChatLog.CreateSystemChatLog($"{message.Name} has connected"));
        }

        public void SendMessage(ChatMessage message)
        {
            _multiplexer.SendMessage(MessagePackage.MakePackage(MessageType.Message, message));
            AddToLog(new ChatLog() { Name = message.Name, Message = message.Message, Type = ChatLogType.Chat });
        }

        public void AddLogNoSend(ChatLog log)
        {
            Log.Add(log);
            LogAdded?.Invoke(this, log);
        }
    }
}
