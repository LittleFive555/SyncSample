using SyncSample.Client.Gameplay;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client
{
    /// <summary>
    /// 客户端示例：连接后发 Ping、处理 Pong/Echo/Chat。挂到场景中并指定 TcpGameClient 引用。
    /// </summary>
    public class GameClientExample : MonoBehaviour
    {
        [SerializeField] private TcpGameClient client;
        [SerializeField] private string playerName = "Player";
        [SerializeField] private float pingInterval = 2f;
        private float _nextPing;

        private void Awake()
        {
            if (client == null) client = GetComponent<TcpGameClient>();
            if (client == null) return;
            GameMain.Instance.Initialize();
            client.OnConnected += OnConnected;
            client.OnDisconnected += OnDisconnected;
            client.OnMessageReceived += OnMessageReceived;
        }

        private void OnDestroy()
        {
            if (client == null) return;
            client.OnConnected -= OnConnected;
            client.OnDisconnected -= OnDisconnected;
            client.OnMessageReceived -= OnMessageReceived;
        }

        private void OnConnected()
        {
            var join = new JoinMessage(string.IsNullOrEmpty(playerName) ? "Guest" : playerName.Trim());
            client.Send(MessageType.Join, JsonUtility.ToJson(join));
            Logger.Log("已连接，已发送名字: " + join.name);
        }

        private void OnDisconnected()
        {
            Logger.Log("已断开");
        }

        private void OnMessageReceived(NetworkEnvelope envelope)
        {
            if (string.IsNullOrEmpty(envelope?.type)) return;
            switch (envelope.type)
            {
                // case MessageType.Pong:
                //     try
                //     {
                //         var pong = JsonUtility.FromJson<PongMessage>(envelope.payload);
                //         long rtt = TimeUtil.UtcNowMillis() - pong.timestamp;
                //         Logger.Log($"Pong 延迟约 {rtt} ms");
                //     }
                //     catch { }
                //     break;
                // case MessageType.Echo:
                //     Logger.Log("Echo: " + envelope.payload);
                //     break;
                // case MessageType.Chat:
                //     try
                //     {
                //         var chat = JsonUtility.FromJson<ChatMessage>(envelope.payload);
                //         Logger.Log($"聊天 [{chat.sender}]: {chat.text}");
                //     }
                //     catch { }
                //     break;
                case MessageType.ClientList:
                    try
                    {
                        var list = JsonUtility.FromJson<ClientListMessage>(envelope.payload);
                        if (list.clients != null)
                        {
                            Logger.Log($"当前在线 {list.clients.Length} 人");
                            for (int i = 0; i < list.clients.Length; i++)
                                Logger.Log($"  - [{list.clients[i].id}] {list.clients[i].name}");
                        }
                    }
                    catch { }
                    break;
                case MessageType.ClientJoined:
                    try
                    {
                        var info = JsonUtility.FromJson<ClientInfo>(envelope.payload);
                        Logger.Log($"新玩家加入: [{info.id}] {info.name}");
                    }
                    catch { }
                    break;
            }
        }

        private void Update()
        {
            if (client == null || !client.IsConnected) return;
            // if (Time.time >= _nextPing)
            // {
            //     _nextPing = Time.time + pingInterval;
            //     var ping = new PingMessage(TimeUtil.UtcNowMillis());
            //     client.Send(MessageType.Ping, JsonUtility.ToJson(ping));
            // }
        }

        /// <summary> 发送 Echo（可由 UI 按钮调用） </summary>
        public void SendEcho(string content)
        {
            if (client == null || !client.IsConnected) return;
            client.Send(MessageType.Echo, JsonUtility.ToJson(new EchoMessage(content)));
        }

        /// <summary> 发送聊天（可由 UI 按钮调用） </summary>
        public void SendChat(string sender, string text)
        {
            if (client == null || !client.IsConnected) return;
            client.Send(MessageType.Chat, JsonUtility.ToJson(new ChatMessage(sender, text)));
        }
    }
}
