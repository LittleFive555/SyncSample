using System;
using System.Collections.Generic;

namespace SyncSample.Common
{
    /// <summary>
    /// 所有客户端的输入。
    /// </summary>
    [Serializable]
    public class AllPlayerInputMessage
    {
        /// <summary> 逻辑帧号，用于在 WaitForAllClientsThisFrame 中按帧生效。 </summary>
        public long frame;
        /// <summary> 所有客户端的输入。 </summary>
        public List<PlayerInputMessage> playerInputs;

        public AllPlayerInputMessage() { }

        public AllPlayerInputMessage(long frame, List<PlayerInputMessage> playerInputs)
        {
            this.frame = frame;
            this.playerInputs = playerInputs;
        }
    }
}