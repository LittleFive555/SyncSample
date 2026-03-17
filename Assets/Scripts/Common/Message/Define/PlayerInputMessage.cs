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
        /// <summary> 本帧 x 轴位移（定点数）。 </summary>
        public FixedPoint dx;
        /// <summary> 本帧 y 轴位移（定点数）。 </summary>
        public FixedPoint dy;

        public PlayerInputMessage() { }

        public PlayerInputMessage(long frame, FixedPoint dx, FixedPoint dy)
        {
            this.frame = frame;
            this.dx = dx;
            this.dy = dy;
        }

        public PlayerInputMessage(long frame, string clientId, FixedPoint dx, FixedPoint dy)
        {
            this.frame = frame;
            this.clientId = clientId ?? string.Empty;
            this.dx = dx;
            this.dy = dy;
        }
    }
}
