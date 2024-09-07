using SimpleChat.Core;
using SimpleChat.Core.Messages;
using SadConsole;

namespace SimpleChat.UI
{
    internal class RootScreen : ScreenObject
    {
        private ScreenObject _currentScreen;
        private MessageEventMultiplexer? multiplexer;
        private ChatRoom? room;
        private ChatNetworkServer? server;
        private readonly LoginScreen _loginScreen;
        private readonly int width;
        private readonly int height;

        public RootScreen(int width, int height) {
            _loginScreen = new LoginScreen(width, height);
            _currentScreen = _loginScreen;

            _loginScreen.RequestJoin += LoginScreen_RequestJoin;
            _loginScreen.RequestHost += LoginScreen_RequestHost;
            Children.Add(_currentScreen);
            this.width = width;
            this.height = height;
        }

        private void LoginScreen_RequestHost(object? sender, LoginScreen.LoginInfo e)
        {
            server = new ChatNetworkServer(int.Parse(e.Port), int.Parse(e.Port));
            server.Start();
            multiplexer = new MessageEventMultiplexer(server);
            room = new ChatRoom(multiplexer, e.UserName);
            var chatRoomScreen = new ChatRoomScreen(width, height, room);
            Children.Remove(_currentScreen);
            _currentScreen = chatRoomScreen;
            Children.Add(_currentScreen);
            room.AddLogNoSend(ChatLog.CreateSystemChatLog($"Started server on port {e.Port} as {e.UserName}"));
            chatRoomScreen.RequestSendMessage += ChatRoomScreen_RequestSendMessage;
        }

        private void ChatRoomScreen_RequestSendMessage(object? sender, string e)
        {
            room?.SendMessage(new ChatMessage() { Message = e, Name = room.UserName });
        }

        private void LoginScreen_RequestJoin(object? sender, LoginScreen.LoginInfo e)
        {
            var chatTransceiver = new ChatTransceiver(e.Server, int.Parse(e.Port));
            multiplexer = new MessageEventMultiplexer(chatTransceiver);
            room = new ChatRoom(multiplexer, e.UserName);
            var chatRoomScreen = new ChatRoomScreen(width, height, room);
            Children.Remove(_currentScreen);
            _currentScreen = chatRoomScreen;
            Children.Add(_currentScreen);
            room.AddLogNoSend(ChatLog.CreateSystemChatLog($"Connected to server {e.Server} on port {e.Port} as {e.UserName}"));
            chatRoomScreen.RequestSendMessage += ChatRoomScreen_RequestSendMessage;
        }
    }
}
