using System;
using System.Collections.Generic;
using SyncSample.Client.Gameplay.World.Logic;
using SyncSample.Common.Model;

namespace SyncSample.Client.Gameplay.StateSync.World.Logic
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
            if (_characterEntities.TryGetValue(id, out var characterEntity) )
                return characterEntity;

            // 先创建数据
            characterEntity = new CharacterEntity(id, displayName, SelfId == id);
            _characterEntities[id] = characterEntity;
            OnPlayerCreated?.Invoke(characterEntity);
            return characterEntity;
        }

        public void RemovePlayer(string id)
        {
            if (_characterEntities.TryGetValue(id, out var characterEntity) && characterEntity != null)
            {
                _characterEntities.Remove(id);
                OnPlayerRemoved?.Invoke(characterEntity);
            }
        }

        /// <summary> 客户端预测使用 </summary>
        public void ReceiveInput(long frame, float dx, float dy)
        {
            if (string.IsNullOrEmpty(SelfId) || !_characterEntities.TryGetValue(SelfId, out var entity) || entity == null)
                return;
            entity.ReceiveInput(frame, dx, dy);
        }

        /// <summary> 根据服务器 WorldState 创建缺失角色并覆盖位置与输入方向。 </summary>
        public void ApplyServerWorldState(MsgCharacterEntity[] characters)
        {
            if (characters == null)
                return;
            for (int i = 0; i < characters.Length; i++)
            {
                var e = characters[i];
                if (e == null || string.IsNullOrEmpty(e.id))
                    continue;
                string displayName = string.IsNullOrEmpty(e.name) ? e.id : e.name;
                var characterEntity = EnsurePlayer(e.id, displayName);
                if (characterEntity == null)
                    continue;
                characterEntity.ReceiveWorldState(e.x, e.y, e.dx, e.dy);
            }
        }
    }
}
