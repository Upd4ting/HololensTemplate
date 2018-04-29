using System;

using Windows.Networking.Sockets;

namespace HololensTemplate.Network {
    public class UniversalTcpServer {
    #if UNITY_EDITOR
        private readonly TcpListener _server;
            #elif UNITY_WSA
        private readonly StreamSocketListener _server;
    #endif
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