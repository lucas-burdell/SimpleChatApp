using SimpleChat.Helpers;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;

namespace SimpleChat.UI
{
    internal class LoginScreen : ControlsConsole
    {
        public event EventHandler<LoginInfo>? RequestJoin;
        public event EventHandler<LoginInfo>? RequestHost;

        public LoginScreen(int width, int height) : base(width, height)
        {
            var usernameBox = new TextboxWithBorder(12, "Username", Environment.UserName, new Point(2, 2));
            var serverBox = new TextboxWithBorder(18, "Server Address", "", new Point(2, 5));
            var portBox = new TextboxWithBorder(9, "Port", "25565", new Point(2 + 18 + 2, 5));

            Controls.Add(usernameBox);
            Controls.Add(serverBox);
            Controls.Add(portBox);

            var joinButton = new ButtonBox(13, 3)
            {
                Text = "Join Server",
                Position = new Point(1, 7),
                UseExtended = true
            };
            joinButton.Click += (s, a) =>
            {
                RequestJoin?.Invoke(this, new LoginInfo() { Port = portBox.Text, Server = serverBox.Text, UserName = usernameBox.Text });
                SadConsole.UI.Window.Message($"Join {usernameBox.Text}: {serverBox.Text}:{portBox.Text}", "Close");
            };
            Controls.Add(joinButton);

            var hostButton = new ButtonBox(13, 3)
            {
                Text = "Host Server",
                Position = new Point(1 + 13 + 1, 7),
                UseExtended = true
            };
            hostButton.Click += (s, a) =>
            {
                RequestHost?.Invoke(this, new LoginInfo() { Port = portBox.Text, Server = serverBox.Text, UserName = usernameBox.Text });
                SadConsole.UI.Window.Message($"Hosting on port {portBox.Text} as {usernameBox.Text}", "Close");
            };
            Controls.Add(hostButton);

            this.FrameTextBoxes();
        }

        internal class LoginInfo
        {
            public required string UserName { get; set; }
            public required string Server { get; set; }
            public required string Port { get; set; }
        }
    }
}
