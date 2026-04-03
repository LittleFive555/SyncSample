using System;
using System.Collections.Generic;
using SyncSample.Client.Gameplay.World.Logic;
using SyncSample.Common;

namespace SyncSample.Client.Gameplay.Lockstep.World.Logic
{
    /// <summary>
    /// 根据连接协议在世界中为每个玩家创建一个 GameObject 表示人物。
    /// 连接成功收到 ClientList 后创建本地及已有玩家；收到 ClientJoined 后创建新加入的玩家。
    /// </summary>
    public class CharacterManager
    {
        private static CharacterManager _instance;
        public static CharacterManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CharacterManager();
                return _instance;
            }
        }

        private readonly Dictionary<string, CharacterEntity> _characterEntities = new Dictionary<string, CharacterEntity>();
        public string SelfId;

        public Action<ICharacterEntity> OnPlayerCreated;
        public Action<ICharacterEntity> OnPlayerRemoved;

        public CharacterEntity EnsurePlayer(string id, string displayName)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_characterEntities.TryGetValue(id, out var characterEntity))
                return characterEntity;

            // 先创建数据
            characterEntity = new CharacterEntity(id, displayName, SelfId == id);
            _characterEntities[id] = characterEntity;
            characterEntity.SetPosition(0, 0f);

            LockstepWorldManager.Instance.RegisterLogicEntity(characterEntity);

            OnPlayerCreated?.Invoke(characterEntity);
            return characterEntity;
        }

        public void RemovePlayer(string id)
        {
            if (_characterEntities.TryGetValue(id, out var characterEntity) && characterEntity != null)
            {
                LockstepWorldManager.Instance.UnregisterLogicEntity(characterEntity);

                _characterEntities.Remove(id);

                OnPlayerRemoved?.Invoke(characterEntity);
            }
        }

        /// <summary> 应用服务器下发的位移，在 WaitForAllClientsThisFrame 中被调用：先应用到逻辑，再同步到显示。 </summary>
        public void ReceiveInput(string clientId, long frame, FixedPoint dx, FixedPoint dy)
        {
            if (string.IsNullOrEmpty(clientId) || !_characterEntities.TryGetValue(clientId, out var entity) || entity == null)
                return;
            entity.ReceiveInput(frame, dx, dy);
        }
    }
}
