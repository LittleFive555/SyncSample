using System;
using System.Collections.Generic;
using SyncSample.Common;
using Random = Unity.Mathematics.Random;

namespace SyncSample.Client.Airplane.Logic
{
    
    /// <summary>
    /// 根据连接协议在世界中为每个玩家创建一个 GameObject 表示人物。
    /// 连接成功收到 ClientList 后创建本地及已有玩家；收到 ClientJoined 后创建新加入的玩家。
    /// </summary>
    public class AirplaneManager
    {
        private static AirplaneManager _instance;
        public static AirplaneManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AirplaneManager();
                return _instance;
            }
        }

        private readonly Dictionary<string, AirplaneEntity> _characterEntities = new Dictionary<string, AirplaneEntity>();
        private readonly Dictionary<long, EnemyEntity> _enemyEntities = new Dictionary<long, EnemyEntity>();
        public IReadOnlyDictionary<long, EnemyEntity> EnemyEntities => _enemyEntities;
        private readonly Dictionary<long, BulletEntity> _bulletEntities = new Dictionary<long, BulletEntity>();
        public string SelfId;

        public Action<AirplaneEntity> OnPlayerCreated;
        public Action<AirplaneEntity> OnPlayerRemoved;

        public Action<EnemyEntity> OnEnemyCreated;
        public Action<EnemyEntity> OnEnemyRemoved;

        public Action<BulletEntity> OnBulletCreated;
        public Action<BulletEntity> OnBulletRemoved;

        private uint _randomIndex = 0;

        public AirplaneEntity EnsurePlayer(string id, string displayName)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_characterEntities.TryGetValue(id, out var characterEntity))
                return characterEntity;

            // 先创建数据
            characterEntity = new AirplaneEntity(id, displayName, SelfId == id);
            _characterEntities[id] = characterEntity;
            characterEntity.SetPosition(0, 0f);

            AirplaneWorldManager.Instance.RegisterLogicEntity(characterEntity);

            OnPlayerCreated?.Invoke(characterEntity);
            return characterEntity;
        }

        public void RemovePlayer(string id)
        {
            if (_characterEntities.TryGetValue(id, out var characterEntity) && characterEntity != null)
            {
                AirplaneWorldManager.Instance.UnregisterLogicEntity(characterEntity);

                _characterEntities.Remove(id);

                OnPlayerRemoved?.Invoke(characterEntity);
            }
        }

        public void NewEnemy()
        {
            var random = Random.CreateFromIndex(_randomIndex++);
            float x = random.NextFloat(-15, 15);
            float y = 30;
            Logger.Log($"NewEnemy: x = {x}");
            EnemyEntity enemy = new EnemyEntity(FixedPoint.FromFloat(x), FixedPoint.FromFloat(y));
            
            _enemyEntities[enemy.Id] = enemy;
            AirplaneWorldManager.Instance.RegisterLogicEntity(enemy);
            OnEnemyCreated?.Invoke(enemy);
        }

        public void RemoveEnemy(EnemyEntity enemy)
        {
            _enemyEntities.Remove(enemy.Id);
            AirplaneWorldManager.Instance.UnregisterLogicEntity(enemy);
            OnEnemyRemoved?.Invoke(enemy);
        }

        public void NewBullet(FixedPoint x, FixedPoint y)
        {
            BulletEntity bullet = new BulletEntity(x, y);
            _bulletEntities[bullet.Id] = bullet;
            AirplaneWorldManager.Instance.RegisterLogicEntity(bullet);
            OnBulletCreated?.Invoke(bullet);
        }

        public void RemoveBullet(BulletEntity bullet)
        {
            _bulletEntities.Remove(bullet.Id);
            AirplaneWorldManager.Instance.UnregisterLogicEntity(bullet);
            OnBulletRemoved?.Invoke(bullet);
        }

        /// <summary> 应用服务器下发的位移，在 WaitForAllClientsThisFrame 中被调用：先应用到逻辑，再同步到显示。 </summary>
        public void ReceiveInput(string clientId, long frame, int input)
        {
            if (string.IsNullOrEmpty(clientId) || !_characterEntities.TryGetValue(clientId, out var entity) || entity == null)
                return;
            entity.ReceiveInput(frame, input);
        }
    }
}