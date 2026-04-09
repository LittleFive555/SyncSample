using System.Collections.Generic;
using SyncSample.Client.Gameplay.Lockstep.World.Logic;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay.Lockstep
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
                                var characterEntity = CharacterManager.Instance.EnsurePlayer(c.id, c.name);
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
                        var characterEntity = CharacterManager.Instance.EnsurePlayer(info.id, info.name);
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
                        if (receiveDelayMs > 0)
                        {
                            GameMain.Instance.GameLooper.RunAfterDelayMilliseconds(receiveDelayMs,
                                () => LockstepPlayerInputSync.AddPending(input));
                        }
                        else
                            LockstepPlayerInputSync.AddPending(input);
                    }
                    catch (System.Exception e)
                    {
                        Logger.LogWarning("PlayerWorldSpawner PlayerInput 解析失败: " + e.Message);
                    }
                    break;
                case MessageType.AllPlayerInput:
                    try
                    {
                        var allPlayerInput = JsonUtility.FromJson<AllPlayerInputMessage>(envelope.payload);
                        int receiveDelayMs = GlobalSwitch.Instance != null ? GlobalSwitch.Instance.AddReceiveDelay : 0;
                        if (receiveDelayMs > 0) // 延迟模拟
                        {
                            GameMain.Instance.GameLooper.RunAfterDelayMilliseconds(
                                receiveDelayMs,
                                () => { LockstepPlayerInputSync.AddPendingMessage(allPlayerInput); });
                        }
                        else
                        {
                            LockstepPlayerInputSync.AddPendingMessage(allPlayerInput);
                        }

                    }
                    catch (System.Exception e)
                    {
                        Logger.LogWarning("PlayerWorldSpawner AllPlayerInput 解析失败: " + e.Message);
                    }
                    break;
            }
        }
    }
}