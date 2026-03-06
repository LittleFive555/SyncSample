using System;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Server
{
    /// <summary>
    /// 服务器消息处理逻辑，供 GameServerRunner 与编辑器面板共用。
    /// </summary>
    public static class GameServerMessageHandlers
    {
        public static void Handle(TcpGameServer server, ClientSession session, NetworkEnvelope envelope)
        {
            if (server == null || envelope == null || string.IsNullOrEmpty(envelope.type)) return;

            switch (envelope.type)
            {
                case MessageType.Ping:
                    HandlePing(session, envelope.payload);
                    break;
                case MessageType.Echo:
                    HandleEcho(session, envelope.payload);
                    break;
                case MessageType.Chat:
                    HandleChat(server, session, envelope.payload);
                    break;
                default:
                    Logger.Log($"未知消息类型: {envelope.type}");
                    break;
            }
        }

        private static void HandlePing(ClientSession session, string payload)
        {
            try
            {
                var ping = JsonUtility.FromJson<PingMessage>(payload);
                var pong = new PongMessage(ping.timestamp, TimeUtil.UtcNowMillis());
                session.Send(MessageType.Pong, JsonUtility.ToJson(pong));
            }
            catch (Exception e)
            {
                Logger.LogWarning("Ping 解析失败: " + e.Message);
            }
        }

        private static void HandleEcho(ClientSession session, string payload)
        {
            session.Send(MessageType.Echo, payload ?? "{}");
        }

        private static void HandleChat(TcpGameServer server, ClientSession session, string payload)
        {
            try
            {
                var chat = JsonUtility.FromJson<ChatMessage>(payload);
                Logger.Log($"聊天 [{chat.sender}]: {chat.text}");
                server.Broadcast(MessageType.Chat, payload);
            }
            catch (Exception e)
            {
                Logger.LogWarning("Chat 解析失败: " + e.Message);
            }
        }
    }
}
