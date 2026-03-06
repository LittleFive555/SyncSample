using System;
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
        [SerializeField] private float pingInterval = 2f;
        private float _nextPing;

        private void Awake()
        {
            if (client == null) client = GetComponent<TcpGameClient>();
            if (client == null) return;
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
            Logger.Log("已连接，可发送 Echo/Chat");
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
                case MessageType.Pong:
                    try
                    {
                        var pong = JsonUtility.FromJson<PongMessage>(envelope.payload);
                        long rtt = TimeUtil.UtcNowMillis() - pong.timestamp;
                        Logger.Log($"Pong 延迟约 {rtt} ms");
                    }
                    catch { }
                    break;
                case MessageType.Echo:
                    Logger.Log("Echo: " + envelope.payload);
                    break;
                case MessageType.Chat:
                    try
                    {
                        var chat = JsonUtility.FromJson<ChatMessage>(envelope.payload);
                        Logger.Log($"聊天 [{chat.sender}]: {chat.text}");
                    }
                    catch { }
                    break;
            }
        }

        private void Update()
        {
            if (client == null || !client.IsConnected) return;
            if (Time.time >= _nextPing)
            {
                _nextPing = Time.time + pingInterval;
                var ping = new PingMessage(TimeUtil.UtcNowMillis());
                client.Send(MessageType.Ping, JsonUtility.ToJson(ping));
            }
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
