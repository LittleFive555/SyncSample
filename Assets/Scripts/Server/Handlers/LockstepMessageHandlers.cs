using System;
using SyncSample.Common;
using SyncSample.Server.Gameplay;
using UnityEngine;

namespace SyncSample.Server
{
    public interface ISyncMessageHandler
    {
        public void HandleJoin(TcpGameServer server, ClientSession session, string payload);
        public void HandlePlayerInput(TcpGameServer server, ClientSession session, string payload);

    }
    public class LockstepMessageHandlers : ISyncMessageHandler
    {
        public void HandleJoin(TcpGameServer server, ClientSession session, string payload)
        {
            try
            {
                var join = JsonUtility.FromJson<JoinMessage>(payload);
                string name = string.IsNullOrEmpty(join.name) ? "Guest" : join.name.Trim();
                session.Name = name;
                Logger.Log($"客户端加入: Id={session.Id}, Name={name}");

                var sessions = server.GetSessionsSnapshot();
                var clients = new ClientInfo[sessions.Length];
                for (int i = 0; i < sessions.Length; i++)
                    clients[i] = new ClientInfo(sessions[i].Id, sessions[i].Name ?? string.Empty);

                var listMsg = new ClientListMessage(clients);
                listMsg.selfId = session.Id;
                session.Send(MessageType.ClientList, JsonUtility.ToJson(listMsg));

                var newClientInfo = new ClientInfo(session.Id, session.Name);
                string joinedPayload = JsonUtility.ToJson(newClientInfo);
                server.BroadcastExcept(session, MessageType.ClientJoined, joinedPayload);

                // NOTE 乐观锁步：当预期客户端数量达到时，启动锁步同步
                if (GlobalSwitch.Instance.OptimisticLockstep && sessions.Length == GlobalSwitch.Instance.ExpectedClientCount)
                    LockstepSyncManager.Instance.Start(server);
            }
            catch (Exception e)
            {
                Logger.LogWarning("Join 解析失败: " + e.Message);
            }
        }
        
        public void HandlePlayerInput(TcpGameServer server, ClientSession session, string payload)
        {
            try
            {
                var input = JsonUtility.FromJson<PlayerInputMessage>(payload);
                input.clientId = session.Id;
                if (GlobalSwitch.Instance.OptimisticLockstep)
                    LockstepSyncManager.Instance.AppendPlayerInput(session.Id, input);
                else
                    server.Broadcast(MessageType.PlayerInput, JsonUtility.ToJson(input));
            }
            catch (Exception e)
            {
                Logger.LogWarning("PlayerInput 解析失败: " + e.Message);
            }
        }
    }
}
