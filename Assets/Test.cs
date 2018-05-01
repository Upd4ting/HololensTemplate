using System.Threading.Tasks;

using HololensTemplate.Network;
using HololensTemplate.Utils;

using UnityEngine;

namespace HololensTemplate {
    public class Test : MonoBehaviour {
        private const string Port = "14269";

        private void Start() {
            Task.Run(async () => {
                Logs.Log($"Starting test.cs");
                await Task.Delay(500);
                await Task.Run(() => {
                    var server = new UniversalTcpServer(Port);
                    server.OnConnexion += async tcpClient => {
                        var obj = await tcpClient.Read<Message>();
                        Logs.Log($"Object readed: {obj}");
                        obj.Str  = $"Ptdr t'es ki ?";
                        obj.Type = 42;
                        await tcpClient.Write(obj);
                    };
                });
                await Task.Delay(2500);
                await Task.Run(async () => {
                    var l = await UniversalTcpClient.GetAvaliable(Port);
                    Logs.Log($"Avaliable ips [{l.Count}]: ");
                    foreach (var v in l) Logs.Log($"{v}");

                    if (l.Count < 0) {
                        if (l.Count == 1) {
                            using (var client = new UniversalTcpClient(l[0], Port))
                            {
                                await client.Write(new Message(0, "Coucou"));
                                Logs.Log($"Writing message\nWaiting for answer");
                                var obj = await client.Read<Message>();
                                Logs.Log($"Answer: {obj}");
                            }
                        }
                    }
                });
            });
        }
    }

    public class Message {
        public Message(int type, string str) {
            Type = type;
            Str  = str;
        }

        public string Str  { get; set; }
        public int    Type { get; set; }

        /// <inheritdoc />
        public override string ToString() {
            return $"Message[Type:{Type};Str:{Str}]";
        }
    }
}