using System.Collections.Generic;
using SyncSample.Client.Airplane.Logic;
using UnityEngine;

namespace SyncSample.Client.Airplane.View
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private Airplane player1Prefab;
        [SerializeField] private Airplane player2Prefab;
        [SerializeField] private Enemy enemyPrefab;
        [SerializeField] private Bullet bulletPrefab;

        private static Spawner _instance;
        public static Spawner Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<Spawner>();
                return _instance;
            }
        }
        
        private readonly Dictionary<string, Airplane> _playerObjects = new Dictionary<string, Airplane>();
        private readonly Dictionary<long, Enemy> _enemyObjects = new Dictionary<long, Enemy>();
        private readonly Dictionary<long, Bullet> _bulletObjects = new Dictionary<long, Bullet>();

        public void EnsurePlayer(AirplaneEntity characterEntity)
        {
            if (characterEntity == null) return;
            if (_playerObjects.ContainsKey(characterEntity.Id))
                return;

            // 再创建显示
            var go = Instantiate(_playerObjects.Count == 0 ? player1Prefab : player2Prefab);
            go.name = characterEntity.Name;
            go.transform.SetParent(transform);

            var character = go.GetComponent<Airplane>();
            character.Init(characterEntity);

            _playerObjects[characterEntity.Id] = character;
        }

        public void RemovePlayer(AirplaneEntity characterEntity)
        {
            if (characterEntity == null) return;
            if (!_playerObjects.TryGetValue(characterEntity.Id, out var character) || character == null)
                return;
            Destroy(character.gameObject);
            _playerObjects.Remove(characterEntity.Id);
        }

        public void EnsureEnemy(EnemyEntity enemyEntity)
        {
            if (enemyEntity == null) return;
            if (_enemyObjects.ContainsKey(enemyEntity.Id))
                return;

            // 再创建显示
            var go = Instantiate(enemyPrefab);
            go.name = "Enemy_" + enemyEntity.Id;
            go.transform.SetParent(transform);

            var enemy = go.GetComponent<Enemy>();
            enemy.Init(enemyEntity);

            _enemyObjects[enemyEntity.Id] = enemy;
        }

        public void RemoveEnemy(EnemyEntity enemyEntity)
        {
            if (enemyEntity == null) return;
            if (!_enemyObjects.TryGetValue(enemyEntity.Id, out var enemy) || enemy == null)
                return;
            Destroy(enemy.gameObject);
            _enemyObjects.Remove(enemyEntity.Id);
        }

        public void EnsureBullet(BulletEntity bulletEntity)
        {
            if (bulletEntity == null) return;
            if (_bulletObjects.ContainsKey(bulletEntity.Id))
                return;

            // 再创建显示
            var go = Instantiate(bulletPrefab);
            go.name = "Bullet_" + bulletEntity.Id;
            go.transform.SetParent(transform);

            var bullet = go.GetComponent<Bullet>();
            bullet.Init(bulletEntity);

            _bulletObjects[bulletEntity.Id] = bullet;
        }

        public void RemoveBullet(BulletEntity bulletEntity)
        {
            if (bulletEntity == null) return;
            if (!_bulletObjects.TryGetValue(bulletEntity.Id, out var bullet) || bullet == null)
                return;
            Destroy(bullet.gameObject);
            _bulletObjects.Remove(bulletEntity.Id);
        }
    }
}