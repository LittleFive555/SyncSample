using System.Collections.Generic;
using SyncSample.Client.Airplane.Logic;
using SyncSample.Common;

namespace SyncSample.Client.Airplane
{
    /// <summary>
    /// 暂存服务器下发的玩家输入，在 WorldManager 收齐本帧所有客户端输入后按帧取出并生效（lockstep）。
    /// </summary>
    public static class AirplanePlayerInputSync
    {
        private static readonly SortedList<long, AllPlayerInputMessage> PendingMessages = new SortedList<long, AllPlayerInputMessage>();
        private static readonly List<PlayerInputMessage> Pending = new List<PlayerInputMessage>();
        private static readonly HashSet<string> AllClientIds = new HashSet<string>();
        private static readonly object ExpectedLock = new object();
        private static int _expectedClientCount;

        public static bool AllClientsConnected()
        {
            return AllClientIds.Count == _expectedClientCount;
        }

        /// <summary> 设置 lockstep 期望的客户端集合（收到 ClientList 时调用）。 </summary>
        public static void SetClients(IEnumerable<string> clientIds)
        {
            lock (ExpectedLock)
            {
                AllClientIds.Clear();
                if (clientIds != null)
                {
                    foreach (var id in clientIds)
                    {
                        if (!string.IsNullOrEmpty(id))
                            AllClientIds.Add(id);
                    }
                }
            }
        }

        /// <summary> 新增一名 lockstep 参与者（收到 ClientJoined 时调用）。 </summary>
        public static void AddClient(string clientId)
        {
            if (string.IsNullOrEmpty(clientId)) return;
            lock (ExpectedLock)
            {
                AllClientIds.Add(clientId);
            }
        }

        /// <summary> 设置 lockstep 期待的客户端数量（达到该数量即视为本帧收齐）。 </summary>
        public static void SetExpectedClientCount(int count)
        {
            _expectedClientCount = count > 0 ? count : 0;
        }

#region 乐观锁步
        public static void AddPendingMessage(AllPlayerInputMessage message)
        {
            if (message == null) return;
            lock (PendingMessages)
            {
                PendingMessages.Add(message.frame, message);
            }
        }

        public static bool TryGetFrameMessage(long frame, out AllPlayerInputMessage message)
        {
            lock (PendingMessages)
            {
                if (PendingMessages.TryGetValue(frame, out message))
                    return true;
                return false;
            }
        }

        public static long GetLastFrame()
        {
            lock (PendingMessages)
            {
                return PendingMessages.Count > 0 ? PendingMessages.Keys[PendingMessages.Count - 1] : -1;
            }
        }

        public static void ApplyFrameMessage(AllPlayerInputMessage message)
        {
            if (message == null) return;
            if (message.playerInputs != null)
            {
                foreach (var e in message.playerInputs)
                    AirplaneManager.Instance.ReceiveInput(e.clientId, e.frame, e.input);
            }
        }
#endregion

#region 原始锁步
        /// <summary> dx, dy 为协议中的定点数，应用时再转为浮点。 </summary>
        public static void AddPending(PlayerInputMessage message)
        {
            lock (Pending)
            {
                Pending.Add(message);
            }
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
                if (AllClientIds.Count == 0)
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
                    return receivedForFrame.IsSupersetOf(AllClientIds);
                }
            }
        }

        /// <summary> 在收齐本帧输入后调用：应用本帧所有待处理输入后移除。 </summary>
        public static void ApplyFrame(long frame)
        {
            List<PlayerInputMessage> toApply;
            lock (Pending)
            {
                toApply = new List<PlayerInputMessage>();
                for (int i = Pending.Count - 1; i >= 0; i--)
                {
                    if (Pending[i].frame == frame)
                    {
                        toApply.Add(Pending[i]);
                        Pending.RemoveAt(i);
                    }
                }
            }
            foreach (var e in toApply)
                AirplaneManager.Instance.ReceiveInput(e.clientId, e.frame, e.input);
        }
#endregion
    }
}
