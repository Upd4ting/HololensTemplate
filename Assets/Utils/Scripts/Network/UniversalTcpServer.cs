using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using HololensTemplate.Utils;

namespace HololensTemplate.Network {
#if UNITY_EDITOR
#elif UNITY_WSA
using Windows.Networking;
    using Windows.Networking.Sockets;
        #endif
    public class UniversalTcpServer {
        public const string Multicast = "226.67.42.3";
#if UNITY_EDITOR
        private readonly TcpListener _server;
#elif UNITY_WSA
        private readonly StreamSocketListener _server;
                        #endif

        private async Task BindToLan(string port) {
#if UNITY_EDITOR
            var p = int.Parse(port);
            using (var socket = new UdpClient(p)) {
                socket.JoinMulticastGroup(IPAddress.Parse(Multicast));
                Logs.Log($"UDP ready");
                while (true) {
                    var rec = await socket.ReceiveAsync();
                    var time = DateTime.Now;
                    Logs.Log($"Udp message received");
                    var str = Encoding.Default.GetString(rec.Buffer);
                    if (str != "IP?") {
                        Logs.LogWarning($"Bad message");
                        continue;
                    }

                    var rep = Encoding.Default.GetBytes("IP!");
                    await socket.SendAsync(rep, rep.Length, rec.RemoteEndPoint);
                    Logs.Log($"Answer sent in {DateTime.Now - time}");
                }
            }
#elif UNITY_WSA
            //using(var _socket = new DatagramSocket()) {
            //    await _socket.BindServiceNameAsync(port);
            //    _socket.JoinMulticastGroup(new HostName(Multicast));
            //    _socket.MessageReceived += async (sender, args) => {
            //        Logs.Log($"Received message from {args.RemoteAddress}:{args.RemotePort}");
            //        using (var output = new DataWriter(await sender.GetOutputStreamAsync(args.RemoteAddress, args.RemotePort))) {
            //            string read;
            //            using (var reader = args.GetDataReader()) { read = reader.ReadString(reader.UnconsumedBufferLength); }

            //            if (read != "IP?") return;
            //            output.WriteString("IP!");
            //            await output.StoreAsync();
            //            await output.FlushAsync();
            //        }
            //    };
            //    while (true) await Task.Delay(new TimeSpan(1, 0, 0, 0));
            //}
#endif
        }

        public UniversalTcpServer(string port) {
            Task.Run(async () => { await BindToLan(port); });
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