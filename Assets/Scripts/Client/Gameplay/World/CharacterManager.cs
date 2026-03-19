using System.Collections.Generic;
using SyncSample.Common;
using SyncSample.Common.Model;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 根据连接协议在世界中为每个玩家创建一个 GameObject 表示人物。
    /// 连接成功收到 ClientList 后创建本地及已有玩家；收到 ClientJoined 后创建新加入的玩家。
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        [SerializeField] private PrimitiveType playerShape = PrimitiveType.Capsule;
        [SerializeField] private Color localPlayerColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color remotePlayerColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private float spawnSpacing = 2f;

        public static CharacterManager Instance { get; private set; }

        private Transform _playersRoot;
        private readonly Dictionary<string, GameObject> _playerObjects = new Dictionary<string, GameObject>();
        public string SelfId;

        private void Awake()
        {
            Instance = this;
            var root = new GameObject("Players");
            root.transform.SetParent(transform);
            _playersRoot = root.transform;
        }

        public void EnsurePlayer(string id, string displayName)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (_playerObjects.ContainsKey(id))
                return;

            var go = GameObject.CreatePrimitive(playerShape);
            go.name = string.IsNullOrEmpty(displayName) ? id : displayName;
            go.transform.SetParent(_playersRoot);

            int index = _playerObjects.Count;
            float startX = index * spawnSpacing;

            var character = go.AddComponent<Character>();
            character.Init(id, displayName, startX, 0f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(renderer.sharedMaterial);
                mat.color = SelfId == id ? localPlayerColor : remotePlayerColor;
                renderer.sharedMaterial = mat;
            }

            _playerObjects[id] = go;
        }

        /// <summary> 应用服务器下发的位移，在 WaitForAllClientsThisFrame 中被调用：先应用到逻辑，再同步到显示。 </summary>
        public void ReceiveInput(string clientId, long frame, FixedPoint dx, FixedPoint dy)
        {
            GameObject go;
            if (string.IsNullOrEmpty(clientId) || !_playerObjects.TryGetValue(clientId, out go) || go == null)
                return;
            var character = go.GetComponent<Character>();
            if (character != null)
                character.ReceiveInput(frame, dx, dy);
        }

        /// <summary> 根据服务器 WorldState 创建缺失角色并覆盖位置与输入方向。 </summary>
        public void ApplyServerWorldState(CharacterEntity[] characters)
        {
            if (characters == null)
                return;
            for (int i = 0; i < characters.Length; i++)
            {
                var e = characters[i];
                if (e == null || string.IsNullOrEmpty(e.id))
                    continue;
                string displayName = string.IsNullOrEmpty(e.name) ? e.id : e.name;
                EnsurePlayer(e.id, displayName);
                if (!_playerObjects.TryGetValue(e.id, out var go) || go == null)
                    continue;
                var ch = go.GetComponent<Character>();
                if (ch != null)
                    ch.ApplyWorldState(e.x, e.y, e.dx, e.dy);
            }
        }
    }
}
