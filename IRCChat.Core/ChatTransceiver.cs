using SimpleChat.Core.Messages;
using System.Net.Sockets;

namespace SimpleChat.Core
{
    public class ChatTransceiver : IDisposable
    {
        public event EventHandler<MessagePackage>? MessageReceived;
        public event EventHandler<MessagePackage>? MessageSent;
        public event EventHandler? ConnectionClosed;

        private CancellationTokenSource cancelListeningSource = new CancellationTokenSource();
        private TcpClient _tcpClient;
        private Task? _listeningTask;

        public ChatTransceiver(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _listeningTask = Task.Run(ListenAsync);
        }

        public ChatTransceiver(string address, int port) : this(new TcpClient(address, port))
        {
        }

        public async Task<bool> TrySendMessageAsync(MessagePackage message)
        {
            if (!_tcpClient.Connected)
            {
                ConnectionClosed?.Invoke(this, EventArgs.Empty);
                return false;
            }
            try
            {
                var stream = _tcpClient.GetStream();
                byte[] sendTypeMessage = new byte[sizeof(int) * 2];
                Array.Copy(BitConverter.GetBytes((int)message.MessageType), 0, sendTypeMessage, 0, 4);
                Array.Copy(BitConverter.GetBytes(message.Data.Length), 0, sendTypeMessage, 4, 4);
                await stream.WriteAsync(sendTypeMessage);
                await stream.FlushAsync();
                byte[] messageData = message.Data;
                await stream.WriteAsync(messageData);
                await stream.FlushAsync();
                MessageSent?.Invoke(this, message);
                return true;
            }
            catch (Exception)
            {
                if (!_tcpClient.Connected)
                {
                    ConnectionClosed?.Invoke(this, EventArgs.Empty);
                }
                return false;
            }
        }

        public void Dispose()
        {
            cancelListeningSource.Cancel();
            _listeningTask?.Dispose();
            _listeningTask = null;
        }

        private async Task ListenAsync()
        {
            var cancellationToken = cancelListeningSource.Token;
            NetworkStream stream = _tcpClient.GetStream();
            while (_tcpClient.Connected && !cancellationToken.IsCancellationRequested)
            {
                await TryReadMessageAsync(stream, cancellationToken);
            }
            ConnectionClosed?.Invoke(this, EventArgs.Empty);
        }

        private async Task<bool> TryReadMessageAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            if (!_tcpClient.Connected)
            {
                return false;
            }
            try
            {
                byte[] messageTypeMessage = new byte[sizeof(int) * 2];
                var bytesRead = await stream.ReadAsync(messageTypeMessage, 0, messageTypeMessage.Length, cancellationToken);
                if (bytesRead == 0)
                {
                    return false;
                }
                var recievedMessage = new MessagePackage();
                byte[] messageTypeBytes = new byte[sizeof(int)];
                byte[] messageSizeBytes = new byte[sizeof(int)];
                Array.Copy(messageTypeMessage, 0, messageTypeBytes, 0, sizeof(int));
                Array.Copy(messageTypeMessage, 4, messageSizeBytes, 0, sizeof(int));
                recievedMessage.MessageType = (MessageType)BitConverter.ToInt32(messageTypeBytes, 0);
                int dataSize = BitConverter.ToInt32(messageSizeBytes, 0);
                recievedMessage.Data = new byte[dataSize];
                await stream.ReadAsync(recievedMessage.Data, 0, dataSize, cancellationToken);
                MessageReceived?.Invoke(this, recievedMessage);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
