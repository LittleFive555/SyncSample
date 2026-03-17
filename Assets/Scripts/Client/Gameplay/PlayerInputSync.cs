using System;
using System.Collections.Generic;
using SyncSample.Common;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 暂存服务器下发的玩家输入，在 WorldManager.WaitForAllClientsThisFrame() 中按帧取出并生效。
    /// </summary>
    public static class PlayerInputSync
    {
        private static readonly List<PlayerInputEntry> Pending = new List<PlayerInputEntry>();
        private static Action<string, float, float> _mover;

        /// <summary> dx, dy 为协议中的定点数，应用时再转为浮点。 </summary>
        public static void AddPending(long frame, string clientId, FixedPoint dx, FixedPoint dy)
        {
            lock (Pending)
            {
                Pending.Add(new PlayerInputEntry(frame, clientId, dx, dy));
            }
        }

        /// <summary> 由 PlayerWorldSpawner 等注册，用于应用位移。 </summary>
        public static void SetMover(Action<string, float, float> mover)
        {
            _mover = mover;
        }

        /// <summary> 在 WaitForAllClientsThisFrame 中调用：应用本帧所有待处理输入后移除。 </summary>
        public static void ApplyFrame(long frame)
        {
            List<PlayerInputEntry> toApply;
            lock (Pending)
            {
                toApply = new List<PlayerInputEntry>();
                for (int i = Pending.Count - 1; i >= 0; i--)
                {
                    if (Pending[i].frame == frame)
                    {
                        toApply.Add(Pending[i]);
                        Pending.RemoveAt(i);
                    }
                }
            }
            if (_mover == null) return;
            foreach (var e in toApply)
                _mover(e.clientId, e.dx.ToFloat(), e.dy.ToFloat());
        }

        private struct PlayerInputEntry
        {
            public long frame;
            public string clientId;
            public FixedPoint dx;
            public FixedPoint dy;

            public PlayerInputEntry(long frame, string clientId, FixedPoint dx, FixedPoint dy)
            {
                this.frame = frame;
                this.clientId = clientId ?? string.Empty;
                this.dx = dx;
                this.dy = dy;
            }
        }
    }
}
