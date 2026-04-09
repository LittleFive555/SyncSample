using System.Collections.Generic;
using SyncSample.Client.Gameplay.World.Logic;
using UnityEngine;

namespace SyncSample.Client.Gameplay.World.View
{
    public class CharacterSpawner : MonoBehaviour
    {
        [SerializeField] private PrimitiveType playerShape = PrimitiveType.Capsule;
        [SerializeField] private Color localPlayerColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color remotePlayerColor = new Color(0.6f, 0.6f, 0.6f);

        private static CharacterSpawner _instance;
        public static CharacterSpawner Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<CharacterSpawner>();
                return _instance;
            }
        }

        private readonly Dictionary<string, Character> _playerObjects = new Dictionary<string, Character>();

        public void EnsurePlayer(ICharacterEntity characterEntity)
        {
            if (characterEntity == null) return;
            if (_playerObjects.ContainsKey(characterEntity.Id))
                return;

            // 再创建显示
            var go = GameObject.CreatePrimitive(playerShape);
            go.name = characterEntity.Name;
            go.transform.SetParent(transform);

            var character = go.AddComponent<Character>();
            character.Init(characterEntity);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(renderer.sharedMaterial);
                mat.color = characterEntity.IsLocal ? localPlayerColor : remotePlayerColor;
                renderer.sharedMaterial = mat;
            }

            _playerObjects[characterEntity.Id] = character;
        }

        public void RemovePlayer(ICharacterEntity characterEntity)
        {
            if (characterEntity == null) return;
            if (!_playerObjects.TryGetValue(characterEntity.Id, out var character) || character == null)
                return;
            Destroy(character.gameObject);
            _playerObjects.Remove(characterEntity.Id);
        }
    }
}
