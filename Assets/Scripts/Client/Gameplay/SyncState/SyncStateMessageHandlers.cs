using System.Collections.Generic;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    public class SyncStateMessageHandlers
    {
        public static void OnMessageReceived(NetworkEnvelope envelope)
        {
            if (string.IsNullOrEmpty(envelope?.type)) return;
            switch (envelope.type)
            {
                case MessageType.ClientList:
                    try
                    {
                        var list = JsonUtility.FromJson<ClientListMessage>(envelope.payload);
                        CharacterManager.Instance.SelfId = list.selfId ?? string.Empty;
                    }
                    catch (System.Exception e)
                    {
                        Logger.LogWarning("PlayerWorldSpawner ClientList 解析失败: " + e.Message);
                    }
                    break;
                case MessageType.ClientJoined:
                    break;
                case MessageType.WorldState:
                    try
                    {
                        var worldState = JsonUtility.FromJson<WorldStateMessage>(envelope.payload);
                        SyncStateWorldManager.Instance.UpdateWorldState(worldState);
                    }
                    catch (System.Exception e)
                    {
                        Logger.LogWarning("PlayerWorldSpawner WorldState 解析失败: " + e.Message);
                    }
                    break;
            }
        }
    }
}