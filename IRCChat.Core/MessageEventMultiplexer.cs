using SimpleChat.Core.Messages;

namespace SimpleChat.Core
{
    public class MessageEventMultiplexer
    {
        public event EventHandler<ConnectedMessage>? OnConnectedMessage;
        public event EventHandler<ChatMessage>? OnChatMessage;
        public event EventHandler<RoomNameMessage>? OnRoomNameMessage;

        public bool IsHost { get => _server != null; }

        private readonly ChatNetworkServer? _server;
        private readonly List<ChatTransceiver> _chatTransceivers;

        public MessageEventMultiplexer(ChatTransceiver chatTransceiver)
        {
            _chatTransceivers = [chatTransceiver];
            SetupEventListeners();
        }

        public MessageEventMultiplexer(ChatNetworkServer server)
        {
            _server = server;
            _chatTransceivers = new List<ChatTransceiver>();
            SetupEventListeners();
        }

        private void SetupEventListenerOnTransceiver(ChatTransceiver chatTransceiver)
        {
            chatTransceiver.MessageReceived += ChatTransceiver_MessageReceived;
            chatTransceiver.ConnectionClosed += ChatTransceiver_ConnectionClosed;
        }

        private void SetupEventListeners()
        {
            foreach (var transceiver in _chatTransceivers)
            {
                SetupEventListenerOnTransceiver(transceiver);
            }
            if (_server != null)
            {
                _server.OnNewConnection += _server_OnNewConnection;
            }
        }

        private void ChatTransceiver_ConnectionClosed(object? sender, EventArgs e)
        {
            if (sender is ChatTransceiver transceiver)
            {
                _chatTransceivers.Remove(transceiver);
            }
        }

        private void _server_OnNewConnection(object? sender, ChatTransceiver e)
        {
            e.MessageReceived += ChatTransceiver_MessageReceived;
            e.ConnectionClosed += ChatTransceiver_ConnectionClosed;
            _chatTransceivers.Add(e);
        }

        private void ChatTransceiver_MessageReceived(object? sender, MessagePackage e)
        {
            switch (e.MessageType)
            {
                case MessageType.Connected:
                    OnConnectedMessage?.Invoke(sender, e.GetMessage<ConnectedMessage>());
                    break;
                case MessageType.Message:
                    OnChatMessage?.Invoke(sender, e.GetMessage<ChatMessage>());
                    break;
                case MessageType.RoomName:
                    OnRoomNameMessage?.Invoke(sender, e.GetMessage<RoomNameMessage>());
                    break;
            }
            if (_server != null)
            {
                MultiplexMessageToClients(sender, e);
            }
        }

        private void MultiplexMessageToClients(object? sender, MessagePackage e)
        {
            foreach (var transceiver in _chatTransceivers)
            {
                if (transceiver != sender)
                {
                    _ = transceiver.TrySendMessageAsync(e);
                }
            }
        }

        public void SendMessage(MessagePackage message)
        {
            MultiplexMessageToClients(null, message);
        }
    }
}
