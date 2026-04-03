using System;
using SyncSample.Common.Model;

namespace SyncSample.Common
{
    /// <summary>
    /// 世界状态：Join 成功后由服务器下发，包含当前所有角色实体。
    /// </summary>
    [Serializable]
    public class WorldStateMessage
    {
        /// <summary>
        /// 逻辑帧号
        /// </summary>
        public long frame;
        /// <summary>
        /// 逻辑帧时间
        /// </summary>
        public float frameDeltaTime;
        /// <summary>
        /// 角色实体列表
        /// </summary>
        public MsgCharacterEntity[] characters;

        public WorldStateMessage() { }

        public WorldStateMessage(MsgCharacterEntity[] characters)
        {
            this.characters = characters ?? Array.Empty<MsgCharacterEntity>();
        }
    }
}
