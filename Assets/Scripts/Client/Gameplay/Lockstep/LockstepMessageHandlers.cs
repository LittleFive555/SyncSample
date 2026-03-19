using System.Collections.Generic;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    public class LockstepMessageHandlers
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
                        if (list.clients != null)
                        {
                            var ids = new List<string>(list.clients.Length);
                            foreach (var c in list.clients)
                            {
                                CharacterManager.Instance.EnsurePlayer(c.id, c.name);
                                if (!string.IsNullOrEmpty(c.id)) ids.Add(c.id);
                            }
                            LockstepPlayerInputSync.SetClients(ids);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Logger.LogWarning("PlayerWorldSpawner ClientList 解析失败: " + e.Message);
                    }
                    break;
                case MessageType.ClientJoined:
                    try
                    {
                        var info = JsonUtility.FromJson<ClientInfo>(envelope.payload);
                        CharacterManager.Instance.EnsurePlayer(info.id, info.name);
                        LockstepPlayerInputSync.AddClient(info.id);
                    }
                    catch (System.Exception e)
                    {
                        Logger.LogWarning("PlayerWorldSpawner ClientJoined 解析失败: " + e.Message);
                    }
                    break;
                case MessageType.PlayerInput:
                    try
                    {
                        var input = JsonUtility.FromJson<PlayerInputMessage>(envelope.payload);
                        // Logger.Log("PlayerWorldSpawner PlayerInput: " + input.frame + " " + input.clientId + " " + input.dx.raw + " " + input.dy.raw);
                        if (string.IsNullOrEmpty(input.clientId))
                            break;
                        int receiveDelayMs = GlobalSwitch.Instance != null ? GlobalSwitch.Instance.AddReceiveDelay : 0;
                        long frame = input.frame;
                        string clientId = input.clientId;
                        FixedPoint dx = input.dx;
                        FixedPoint dy = input.dy;
                        if (receiveDelayMs > 0)
                        {
                            GameLooper.Instance.RunAfterDelayMilliseconds(receiveDelayMs,
                                () => LockstepPlayerInputSync.AddPending(frame, clientId, dx, dy));
                        }
                        else
                            LockstepPlayerInputSync.AddPending(frame, clientId, dx, dy);
                    }
                    catch (System.Exception e)
                    {
                        Logger.LogWarning("PlayerWorldSpawner PlayerInput 解析失败: " + e.Message);
                    }
                    break;
            }
        }
    }
}