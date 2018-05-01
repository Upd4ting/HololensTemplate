namespace HololensTemplate.Network {
    using System;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
#if UNITY_EDITOR
    using System.Net.Sockets;

#elif UNITY_WSA
    using Windows.Networking.Sockets;

#endif
    public class UniversalTcpServer {
        public const string Multicast = "226.67.42.3";
        public const string Port      = "14269";
    #if UNITY_EDITOR
        private readonly TcpListener _server;
    #elif UNITY_WSA
        private readonly StreamSocketListener _server;
        #endif

        private async void BindToLan() {
        #if UNITY_EDITOR
            using (var socket = new UdpClient(int.Parse(Port))) {
                socket.JoinMulticastGroup(IPAddress.Parse(Multicast));
                while (true) {
                    var rec = await socket.ReceiveAsync();
                    var str = Encoding.Default.GetString(rec.Buffer);
                    if (str != "IP?") continue;
                    var rep = Encoding.Default.GetBytes("IT'S OK MY FRIEND");
                    await socket.SendAsync(rep, rep.Length, rec.RemoteEndPoint);
                }
            }
        #elif UNITY_WSA
            using(var _socket = new DatagramSocket())
            {
                await _socket.BindServiceNameAsync(Port);
                _socket.JoinMulticastGroup(new HostName(Multicast));
                Logs.Log($"Now listening at {Multicast}:{Port}");
                _socket.MessageReceived += async (sender, args) => {
                    Logs.Log($"Received message from {args.RemoteAddress}:{args.RemotePort}");
                    using (var output = new DataWriter(await sender.GetOutputStreamAsync(args.RemoteAddress, args.RemotePort))) {
                        string read;
                        using (var reader = args.GetDataReader()) { read = reader.ReadString(reader.UnconsumedBufferLength); }

                        if (read != "IP?") return;
                        output.WriteString("IT'S OK MY FRIEND");
                        await output.StoreAsync();
                        await output.FlushAsync();
                    }
                };
                while (true) await Task.Delay(new TimeSpan(1, 0, 0, 0));
            }
                    #endif
        }

        public UniversalTcpServer(string port) {
        #if UNITY_EDITOR
            _server = TcpListener.Create(int.Parse(port));
            _server.Start();
            Task.Run(async () => {
                while (true) {
                    var client = await _server.AcceptTcpClientAsync();
                    OnConnexion?.Invoke(new UniversalTcpClient(client));
                }
            });
        #elif UNITY_WSA
            _server = new StreamSocketListener();
            Bind(port);
            _server.ConnectionReceived += (sender, args) => { OnConnexion?.Invoke(new UniversalTcpClient(args.Socket)); };
                #endif
        }
    #if !UNITY_EDITOR && UNITY_WSA
        private async void Bind(string port) {
            await _server.BindServiceNameAsync(port);
        }
        #endif

        public event Action<UniversalTcpClient> OnConnexion;
    }
}