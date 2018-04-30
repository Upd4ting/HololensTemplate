using System.Collections;
using System.Threading.Tasks;

using HololensTemplate.Network;
using HololensTemplate.Utils;

using UnityEngine;

public class Temp : MonoBehaviour {
    public const string Port = "14269";
    private UniversalTcpServer server;
    private UniversalTcpClient client;
    public void Start() {
        Task.Run(() => {
            server = new UniversalTcpServer(Port);
            server.OnConnexion += async client => {
                using (client) {
                    for (int i = 0; i < 100000; i++) {
                        var message = await client.Read<Message>();
                        Logs.Log($"Message : {message}");
                    }
                }
            };
        });
        Task.Delay(500);
        Task.Run(async () => {
            using (client = new UniversalTcpClient("169.254.151.17", Port)) {
                for (int i = 0; i < 100000; i++)
                    await client.Write(new Message(i + 1, "COUCOU"));
                await Task.Delay(1);
            }
        });
    }
}

public class Message {
    public int    Type { get; set; }
    public string Str  { get; set; }

    public Message(int type, string str) {
        Type = type;
        Str  = str;
    }

    /// <inheritdoc />
    public override string ToString() {
        return $"Message[Type: {Type};Str:\"{Str}\"]";
    }
}