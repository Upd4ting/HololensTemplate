﻿#if UNITY_EDITOR
using System.Net.Sockets;
#elif UNITY_WSA
#endif
namespace HololensTemplate.Network {
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Windows.Networking;
    using Windows.Networking.Sockets;

    using Newtonsoft.Json;

    using UnityEngine;

    using Utils;

    public class UniversalTcpClient : IDisposable {
    #if UNITY_EDITOR
        private readonly TcpClient _client;
    #elif UNITY_WSA
        private readonly StreamSocket _socket;
    #endif
        private StreamReader _reader;
        private StreamWriter _writer;

        private readonly JsonSerializer _serializer;

        private UniversalTcpClient() => _serializer = new JsonSerializer();
    #if UNITY_EDITOR
        internal UniversalTcpClient(TcpClient client) : this() {
            _client = client;
            CreateStreams();
        }
    #elif UNITY_WSA
        internal UniversalTcpClient(StreamSocket socket) : this() {
            _socket = socket;
            CreateStreams();
        }
    #endif

        public UniversalTcpClient(string ip, string port) : this() {
        #if UNITY_EDITOR
            _client = new TcpClient(ip, int.Parse(port));
            #elif UNITY_WSA
            _socket = new StreamSocket();
            Connect(new HostName(ip), port);
            CreateStreams();
        #endif
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
            _writer = new StreamWriter(stream) { AutoFlush = true };
                                #elif UNITY_WSA
            _writer = new StreamWriter(_socket.OutputStream.AsStreamForWrite()) {AutoFlush = true};
            _reader = new StreamReader(_socket.InputStream.AsStreamForRead());
        #endif
        }

        public async Task Write(object obj) {
            var stream = new StringWriter();
            _serializer.Serialize(stream, obj);
            Debug.Log($"Data to write: {stream}");
            await _writer.WriteLineAsync(stream.ToString());
        }

        public async Task<object> Read() => await Read<object>();

        public async Task<T> Read<T>() {
            var str = await _reader.ReadLineAsync();
            Debug.Log($"Read data: {str}");
            var reader = new JsonTextReader(new StringReader(str));
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