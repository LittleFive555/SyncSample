using System;

namespace SyncSample.Common
{
    /// <summary>
    /// 玩家移动输入。客户端发送时可不填 clientId（由服务器填充后广播）；收到时含 clientId 表示谁的操作。
    /// dx、dy 为定点数。
    /// </summary>
    [Serializable]
    public class PlayerInputMessage
    {
        /// <summary> 逻辑帧号，用于在 WaitForAllClientsThisFrame 中按帧生效。 </summary>
        public long frame;
        /// <summary> 客户端 Id，服务器广播时填充。 </summary>
        public string clientId;
        /// <summary> 本帧输入。 </summary>
        public int input;

        public PlayerInputMessage() { }

        public PlayerInputMessage(long frame, int input)
        {
            this.frame = frame;
            this.input = input;
        }

        public PlayerInputMessage(long frame, string clientId, int input)
        {
            this.frame = frame;
            this.clientId = clientId ?? string.Empty;
            this.input = input;
        }
    }
}
