using SimpleChat.Core;
using SimpleChat.Helpers;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadConsole.UI.Controls;

namespace SimpleChat.UI
{
    public class ChatRoomScreen : ControlsConsole, IDisposable
    {
        private readonly ChatRoom _room;
        private readonly SadConsole.Console MessageBuffer;
        private readonly TextboxWithBorder _input;
        private readonly Button _sendButton;
        private readonly ScrollBar _scrollBar;

        public event EventHandler<string>? RequestSendMessage;

        private int _scrollOffset;
        private int _lastCursorY;
        private bool _allowInput;

        public ChatRoomScreen(int width, int height, ChatRoom room) : base(width, height)
        {
            _room = room;
            this.Print(0, 0, $"Room: {room.RoomName}. Connected as {room.UserName}");
            MessageBuffer = new SadConsole.Console(width - 1, height - 3, width - 1, 40);
            MessageBuffer.Position = (0, 1);
            MessageBuffer.UseMouse = false;
            MessageBuffer.UseKeyboard = false;

            _scrollBar = new ScrollBar(Orientation.Vertical, height - 3);
            _scrollBar.IsEnabled = false;
            _scrollBar.ValueChanged += (sender, e) => MessageBuffer.ViewPosition = (0, _scrollBar.Value);
            _scrollBar.Position = (width - 1, 0);
            Controls.Add(_scrollBar);

            _input = new TextboxWithBorder(width - 10, "input", "", (0, height - 3));
            _sendButton = new Button(10, 3);
            _sendButton.Position = (width - 10, height - 3);
            _sendButton.Text = "Send";
            _sendButton.Click += (sender, e) => RequestSendMessage?.Invoke(this, _input.Text);
            Controls.Add(_input);
            Controls.Add(_sendButton);
            Children.Add(MessageBuffer);

            room.LogAdded += Room_LogAdded;

            this.FrameTextBoxes();
        }

        private void Room_LogAdded(object? sender, ChatLog e)
        {
            switch (e.Type)
            {
                case ChatLogType.Chat:
                    MessageBuffer.Cursor.Print($"{e.Name} {e.DateTime.ToString("g")}")
                        .NewLine()
                        .Print(e.Message)
                        .NewLine();
                    break;
                case ChatLogType.System:
                    MessageBuffer.Cursor.Print($"System: {e.Message} {e.DateTime.ToString("g")}").NewLine();
                    break;
                case ChatLogType.Image:
                    break;
            }
        }

        public override void Update(TimeSpan delta)
        {
            // If cursor has moved below the visible area, track the difference
            if (MessageBuffer.Cursor.Position.Y > _scrollOffset + MessageBuffer.ViewHeight - 1)
                _scrollOffset = MessageBuffer.Cursor.Position.Y - MessageBuffer.ViewHeight + 1;

            // Adjust the scroll bar
            _scrollBar.IsEnabled = _scrollOffset != 0;
            _scrollBar.MaximumValue = _scrollOffset;

            // If autoscrolling is enabled, scroll
            if (_scrollBar.IsEnabled && _lastCursorY != MessageBuffer.Cursor.Position.Y)
            {
                _scrollBar.Value = _scrollBar.MaximumValue;
                _lastCursorY = MessageBuffer.Cursor.Position.Y;
            }

            // Update the base class which includes the controls
            base.Update(delta);
        }

        //public override bool ProcessKeyboard(Keyboard keyboard)
        //{
        //    return MessageBuffer.ProcessKeyboard(keyboard);
        //}

        public override bool ProcessMouse(MouseScreenObjectState state)
        {
            if (state.Mouse.ScrollWheelValueChange != 0)
                return _scrollBar.ProcessMouseWheel(state);

            return base.ProcessMouse(state);
        }
    }
}
