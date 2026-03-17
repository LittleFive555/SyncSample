using System;
using System.Collections.Generic;
using SyncSample.Common;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 暂存服务器下发的玩家输入，在 WorldManager 收齐本帧所有客户端输入后按帧取出并生效（lockstep）。
    /// </summary>
    public static class PlayerInputSync
    {
        private static readonly List<PlayerInputEntry> Pending = new List<PlayerInputEntry>();
        private static readonly HashSet<string> ExpectedClientIds = new HashSet<string>();
        private static readonly object ExpectedLock = new object();
        private static int _expectedClientCount;
        private static Action<string, float, float> _mover;

        public static bool AllClientsConnected()
        {
            return ExpectedClientIds.Count == _expectedClientCount;
        }

        /// <summary> dx, dy 为协议中的定点数，应用时再转为浮点。 </summary>
        public static void AddPending(long frame, string clientId, FixedPoint dx, FixedPoint dy)
        {
            lock (Pending)
            {
                Pending.Add(new PlayerInputEntry(frame, clientId, dx, dy));
            }
        }

        /// <summary> 设置 lockstep 期望的客户端集合（收到 ClientList 时调用）。 </summary>
        public static void SetExpectedClients(IEnumerable<string> clientIds)
        {
            lock (ExpectedLock)
            {
                ExpectedClientIds.Clear();
                if (clientIds != null)
                {
                    foreach (var id in clientIds)
                    {
                        if (!string.IsNullOrEmpty(id))
                            ExpectedClientIds.Add(id);
                    }
                }
            }
        }

        /// <summary> 新增一名 lockstep 参与者（收到 ClientJoined 时调用）。 </summary>
        public static void AddExpectedClient(string clientId)
        {
            if (string.IsNullOrEmpty(clientId)) return;
            lock (ExpectedLock)
            {
                ExpectedClientIds.Add(clientId);
            }
        }

        /// <summary> 设置 lockstep 期待的客户端数量（达到该数量即视为本帧收齐）。 </summary>
        public static void SetExpectedClientCount(int count)
        {
            _expectedClientCount = count > 0 ? count : 0;
        }

        /// <summary> 本帧是否已收齐输入：若设置了期待数量则按数量判断，否则按期望客户端 ID 集合判断；均未设置时返回 true。 </summary>
        public static bool HasAllInputsForFrame(long frame)
        {
            if (_expectedClientCount > 0)
            {
                lock (Pending)
                {
                    var receivedForFrame = new HashSet<string>();
                    foreach (var e in Pending)
                    {
                        if (e.frame == frame && !string.IsNullOrEmpty(e.clientId))
                            receivedForFrame.Add(e.clientId);
                    }
                    return receivedForFrame.Count >= _expectedClientCount;
                }
            }
            lock (ExpectedLock)
            {
                if (ExpectedClientIds.Count == 0)
                    return true;
            }
            lock (Pending)
            {
                var receivedForFrame = new HashSet<string>();
                foreach (var e in Pending)
                {
                    if (e.frame == frame)
                        receivedForFrame.Add(e.clientId);
                }
                lock (ExpectedLock)
                {
                    return receivedForFrame.IsSupersetOf(ExpectedClientIds);
                }
            }
        }

        /// <summary> 由 PlayerWorldSpawner 等注册，用于应用位移。 </summary>
        public static void SetMover(Action<string, float, float> mover)
        {
            _mover = mover;
        }

        /// <summary> 在收齐本帧输入后调用：应用本帧所有待处理输入后移除。 </summary>
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
