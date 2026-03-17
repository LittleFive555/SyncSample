using System.Collections.Generic;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 根据连接协议在世界中为每个玩家创建一个 GameObject 表示人物。
    /// 连接成功收到 ClientList 后创建本地及已有玩家；收到 ClientJoined 后创建新加入的玩家。
    /// </summary>
    public class PlayerWorldSpawner : MonoBehaviour
    {
        [SerializeField] private TcpGameClient client;
        [SerializeField] private PrimitiveType playerShape = PrimitiveType.Capsule;
        [SerializeField] private Color localPlayerColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color remotePlayerColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private float spawnSpacing = 2f;

        private Transform _playersRoot;
        private readonly Dictionary<string, GameObject> _playerObjects = new Dictionary<string, GameObject>();
        private string _selfId;

        private void Awake()
        {
            if (client == null) client = FindObjectOfType<TcpGameClient>();
            if (client == null) return;
            var root = new GameObject("Players");
            root.transform.SetParent(transform);
            _playersRoot = root.transform;
            client.OnMessageReceived += OnMessageReceived;
            client.OnDisconnected += OnDisconnected;
        }

        private void OnDestroy()
        {
            if (client == null) return;
            client.OnMessageReceived -= OnMessageReceived;
            client.OnDisconnected -= OnDisconnected;
        }

        private void OnMessageReceived(NetworkEnvelope envelope)
        {
            if (string.IsNullOrEmpty(envelope?.type)) return;
            switch (envelope.type)
            {
                case MessageType.ClientList:
                    try
                    {
                        var list = JsonUtility.FromJson<ClientListMessage>(envelope.payload);
                        _selfId = list.selfId ?? string.Empty;
                        if (list.clients != null)
                        {
                            foreach (var c in list.clients)
                                EnsurePlayer(c.id, c.name, c.id == _selfId);
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
                        EnsurePlayer(info.id, info.name, false);
                    }
                    catch (System.Exception e)
                    {
                        Logger.LogWarning("PlayerWorldSpawner ClientJoined 解析失败: " + e.Message);
                    }
                    break;
            }
        }

        private void OnDisconnected()
        {
            foreach (var go in _playerObjects.Values)
            {
                if (go != null) Destroy(go);
            }
            _playerObjects.Clear();
            _selfId = null;
        }

        private void EnsurePlayer(string id, string displayName, bool isLocal)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (_playerObjects.ContainsKey(id))
                return;

            var go = GameObject.CreatePrimitive(playerShape);
            go.name = string.IsNullOrEmpty(displayName) ? id : displayName;
            go.transform.SetParent(_playersRoot);

            int index = _playerObjects.Count;
            go.transform.localPosition = new Vector3(index * spawnSpacing, 0f, 0f);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(renderer.sharedMaterial);
                mat.color = isLocal ? localPlayerColor : remotePlayerColor;
                renderer.sharedMaterial = mat;
            }

            _playerObjects[id] = go;
        }
    }
}
