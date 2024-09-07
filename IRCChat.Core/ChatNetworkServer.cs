using Mono.Nat;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace SimpleChat.Core
{
    public class ChatNetworkServer : IDisposable
    {
        public event EventHandler? OnSetupComplete;
        public event EventHandler<ChatTransceiver>? OnNewConnection;
        public bool Listening { get => _listening; }
        public bool UseUpnp { get; set; }

        private int _externalPort;
        private int _internalPort;
        private bool _listening = false;
        private TcpListener? _tcpListener = null;
        private Task? _listenerTask = null;
        private readonly ConcurrentDictionary<string, TcpClient> _clients = new ConcurrentDictionary<string, TcpClient>();
        private readonly ConcurrentDictionary<string, ChatTransceiver> _chatTransceivers = new ConcurrentDictionary<string, ChatTransceiver>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public ChatNetworkServer(int externalPort, int internalPort, bool useUpnp = true)
        {
            _externalPort = externalPort;
            _internalPort = internalPort;
            UseUpnp = useUpnp; 
        }

        public void Start()
        {
            NatUtility.DeviceFound += DeviceFound;
            NatUtility.DeviceLost += DeviceLost;
            NatUtility.StartDiscovery();
        }

        private void DeviceFound(object? sender, DeviceEventArgs args)
        {
            INatDevice device = args.Device;
            device.CreatePortMap(new Mapping(Protocol.Tcp, _internalPort, _externalPort));

            StartListening();
        }

        private void DeviceLost(object? sender, DeviceEventArgs args)
        {
            INatDevice device = args.Device;
        }

        private void StartListening()
        {
            _tcpListener = new TcpListener(IPAddress.Any, _internalPort);
            _tcpListener.Start();
            var cancelToken = _cancellationTokenSource.Token;
            _listenerTask = Task.Run(() => ListenForNewConnections(cancelToken), cancelToken);
            _listening = true;

            OnSetupComplete?.Invoke(this, EventArgs.Empty);
        }

        private async Task ListenForNewConnections(CancellationToken cancelToken)
        {
            while (_listening && !cancelToken.IsCancellationRequested)
            {
                if (_tcpListener == null)
                {
                    break;
                }
                TcpClient client = await _tcpListener.AcceptTcpClientAsync(cancelToken);
                IPEndPoint? endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                if (endpoint == null)
                {
                    continue;
                }
                string remoteAddress = endpoint.Address.ToString() + ":" + endpoint.Port;
                _clients.AddOrUpdate(remoteAddress, client,
                    (key, old) =>
                    {
                        old.Dispose();
                        return client;
                    }
                );
                ChatTransceiver chatTransceiver = new ChatTransceiver(client);
                _chatTransceivers.AddOrUpdate(remoteAddress, chatTransceiver,
                    (key, old) =>
                    {
                        old.Dispose();
                        return chatTransceiver;
                    }
                );
                OnNewConnection?.Invoke(this, chatTransceiver);
            }
        }

        public void Dispose()
        {
            _tcpListener?.Dispose();
        }
    }
}

