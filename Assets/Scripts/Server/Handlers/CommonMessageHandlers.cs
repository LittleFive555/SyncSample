using System;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Server
{
    public static class CommonMessageHandlers
    {

        public static void HandlePing(ClientSession session, string payload)
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

        public static void HandleEcho(ClientSession session, string payload)
        {
            session.Send(MessageType.Echo, payload ?? "{}");
        }

        public static void HandleChat(TcpGameServer server, ClientSession session, string payload)
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
