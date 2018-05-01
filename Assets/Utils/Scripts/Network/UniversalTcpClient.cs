using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using HololensTemplate.Utils;

using Newtonsoft.Json;

using UnityEngine;

namespace HololensTemplate.Network {
#if UNITY_EDITOR

    using System.Timers;
    using System.Threading;
    using System.Net;
    using System.Net.Sockets;
#elif UNITY_WSA
    using Windows.Networking;
    using Windows.Networking.Sockets;
#endif

    public class UniversalTcpClient : IDisposable {
#if UNITY_EDITOR
        private readonly TcpClient _client;
#elif UNITY_WSA
        private readonly StreamSocket _socket;
                #endif
        private StreamReader _reader;
        private StreamWriter _writer;

        private readonly JsonSerializer _serializer = new JsonSerializer();

        public static async Task<List<string>> GetAvaliable(string port) {
            var list = new List<string>();
#if UNITY_EDITOR
            using (var socket = new UdpClient {Ttl = 1}) {
                var timer = new System.Timers.Timer {
                    AutoReset = false,
                    Interval  = 1000
                };

                var str          = Encoding.Default.GetBytes("IP?");
                var ct           = new CancellationTokenSource();
                var eventhandler = new EventWaitHandle(false, EventResetMode.ManualReset);
                timer.Elapsed += (sender, args) => { ct.Cancel(); };

                ct.Token.Register(async () => {
                    while (socket.Available > 0) {
                        var rec = await socket.ReceiveAsync();
                        list.Add(rec.RemoteEndPoint.Address.ToString());
                    }

                    eventhandler.Set();
                });

                await socket.SendAsync(str, str.Length, UniversalTcpServer.Multicast, int.Parse(port));
                timer.Start();
                var t = Task.Run(async () => {
                    while (true) {
                        var rec = await socket.ReceiveAsync();
                        list.Add(rec.RemoteEndPoint.Address.ToString());
                    }
                }, ct.Token);

                eventhandler.WaitOne();
            }
#elif UNITY_WSA
#endif
            return list;
        }

#if UNITY_EDITOR
        internal UniversalTcpClient(TcpClient client) {
            _client = client;
            CreateStreams();
        }
#elif UNITY_WSA
        internal UniversalTcpClient(StreamSocket socket){
            _socket = socket;
            CreateStreams();
        }
        #endif

        public UniversalTcpClient(string ip, string port) {
#if UNITY_EDITOR
            _client = new TcpClient(ip, int.Parse(port));
#elif UNITY_WSA
            _socket = new StreamSocket();
            Connect(new HostName(ip), port);
#endif
            CreateStreams();
        }
#if !UNITY_EDITOR && UNITY_WSA
        private async void Connect(HostName hostname, string port) {
            await _socket.ConnectAsync(hostname, port);
        }
                                                                                        #endif
        private void CreateStreams() {
#if UNITY_EDITOR
            var stream = _client.GetStream();
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream) {AutoFlush = true};
#elif UNITY_WSA
            _writer = new StreamWriter(_socket.OutputStream.AsStreamForWrite()) {AutoFlush = true};
            _reader = new StreamReader(_socket.InputStream.AsStreamForRead());
                                                            #endif
        }

        public async Task Write(object obj) {
            var stream = new StringWriter();
            _serializer.Serialize(stream, obj);
            var str = stream.ToString();
            Debug.Log($"Data to write: {str}");
            await _writer.WriteLineAsync(str);
        }

        public async Task<object> Read() {
            return await Read<object>();
        }

        public async Task<T> Read<T>() {
            var value = await _reader.ReadLineAsync();
            Debug.Log($"Read data: {value}");
            Debug.Log($"Lenght: {value.Length}");
            Debug.Log($"Is null or whitespace: {string.IsNullOrWhiteSpace(value)}");
            if (string.IsNullOrWhiteSpace(value)) throw new IOException("Returned data is empty");
            var reader = new JsonTextReader(new StringReader(value));
            return _serializer.Deserialize<T>(reader);
        }

        /// <inheritdoc />
        public void Dispose() {
            _reader?.Dispose();
            _writer?.Dispose();
#if UNITY_EDITOR
            _client?.Dispose();
#elif UNITY_WSA
            _socket?.Dispose();
                                                #endif
        }
    }
}