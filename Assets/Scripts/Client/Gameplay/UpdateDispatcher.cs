using System;
using System.Collections.Generic;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 客户端 Update 调度器：集中驱动 IUpdatable / IFixedUpdatable / ILateUpdatable，
    /// 对每个处理器单独 try-catch，单点异常不影响其他模块，并可选择连续失败后自动禁用。
    /// </summary>
    public class UpdateDispatcher
    {
        private readonly List<IUpdatable> _updateHandlers = new List<IUpdatable>();
        private readonly List<IFixedUpdatable> _fixedUpdateHandlers = new List<IFixedUpdatable>();
        private readonly List<ILateUpdatable> _lateUpdateHandlers = new List<ILateUpdatable>();

        /// <summary> 某处理器连续抛出此次数后将被自动移除，0 表示不自动移除 </summary>
        public int MaxConsecutiveFailuresBeforeDisable { get; set; } = 0;

        private readonly Dictionary<object, int> _updateFailCount = new Dictionary<object, int>();
        private readonly Dictionary<object, int> _fixedFailCount = new Dictionary<object, int>();
        private readonly Dictionary<object, int> _lateFailCount = new Dictionary<object, int>();

        public void Register(IUpdatable handler)
        {
            if (handler == null) return;
            lock (_updateHandlers)
            {
                if (!_updateHandlers.Contains(handler))
                    _updateHandlers.Add(handler);
            }
        }

        public void Register(IFixedUpdatable handler)
        {
            if (handler == null) return;
            lock (_fixedUpdateHandlers)
            {
                if (!_fixedUpdateHandlers.Contains(handler))
                    _fixedUpdateHandlers.Add(handler);
            }
        }

        public void Register(ILateUpdatable handler)
        {
            if (handler == null) return;
            lock (_lateUpdateHandlers)
            {
                if (!_lateUpdateHandlers.Contains(handler))
                    _lateUpdateHandlers.Add(handler);
            }
        }

        /// <summary> 注册实现了多个接口的同一对象 </summary>
        public void Register(object handler)
        {
            if (handler is IUpdatable u) Register(u);
            if (handler is IFixedUpdatable fu) Register(fu);
            if (handler is ILateUpdatable lu) Register(lu);
        }

        public void Unregister(IUpdatable handler)
        {
            if (handler == null) return;
            lock (_updateHandlers) _updateHandlers.Remove(handler);
            _updateFailCount.Remove(handler);
        }

        public void Unregister(IFixedUpdatable handler)
        {
            if (handler == null) return;
            lock (_fixedUpdateHandlers) _fixedUpdateHandlers.Remove(handler);
            _fixedFailCount.Remove(handler);
        }

        public void Unregister(ILateUpdatable handler)
        {
            if (handler == null) return;
            lock (_lateUpdateHandlers) _lateUpdateHandlers.Remove(handler);
            _lateFailCount.Remove(handler);
        }

        public void Unregister(object handler)
        {
            if (handler is IUpdatable u) Unregister(u);
            if (handler is IFixedUpdatable fu) Unregister(fu);
            if (handler is ILateUpdatable lu) Unregister(lu);
        }

        /// <summary> 由 GameMain.Update 调用 </summary>
        public void DispatchUpdate(float deltaTime)
        {
            IUpdatable[] snapshot;
            lock (_updateHandlers) { snapshot = _updateHandlers.ToArray(); }
            foreach (var h in snapshot)
            {
                try
                {
                    h.OnUpdate(deltaTime);
                    _updateFailCount[h] = 0;
                }
                catch (Exception ex)
                {
                    int c;
                    if (!_updateFailCount.TryGetValue(h, out c)) c = 0;
                    int count = c + 1;
                    _updateFailCount[h] = count;
                    Logger.LogError($"[Update] {h.GetType().Name}.OnUpdate 异常 (连续第 {count} 次): {ex.Message}\n{ex.StackTrace}");
                    if (MaxConsecutiveFailuresBeforeDisable > 0 && count >= MaxConsecutiveFailuresBeforeDisable)
                    {
                        Unregister(h);
                        Logger.LogWarning($"[Update] 已自动移除 {h.GetType().Name}");
                    }
                }
            }
        }

        /// <summary> 由 GameMain.FixedUpdate 调用 </summary>
        public void DispatchFixedUpdate(float fixedDeltaTime)
        {
            IFixedUpdatable[] snapshot;
            lock (_fixedUpdateHandlers) { snapshot = _fixedUpdateHandlers.ToArray(); }
            foreach (var h in snapshot)
            {
                try
                {
                    h.OnFixedUpdate(fixedDeltaTime);
                    _fixedFailCount[h] = 0;
                }
                catch (Exception ex)
                {
                    int c;
                    if (!_fixedFailCount.TryGetValue(h, out c)) c = 0;
                    int count = c + 1;
                    _fixedFailCount[h] = count;
                    Logger.LogError($"[FixedUpdate] {h.GetType().Name}.OnFixedUpdate 异常 (连续第 {count} 次): {ex.Message}\n{ex.StackTrace}");
                    if (MaxConsecutiveFailuresBeforeDisable > 0 && count >= MaxConsecutiveFailuresBeforeDisable)
                    {
                        Unregister(h);
                        Logger.LogWarning($"[FixedUpdate] 已自动移除 {h.GetType().Name}");
                    }
                }
            }
        }

        /// <summary> 由 GameMain.LateUpdate 调用 </summary>
        public void DispatchLateUpdate(float deltaTime)
        {
            ILateUpdatable[] snapshot;
            lock (_lateUpdateHandlers) { snapshot = _lateUpdateHandlers.ToArray(); }
            foreach (var h in snapshot)
            {
                try
                {
                    h.OnLateUpdate(deltaTime);
                    _lateFailCount[h] = 0;
                }
                catch (Exception ex)
                {
                    int c;
                    if (!_lateFailCount.TryGetValue(h, out c)) c = 0;
                    int count = c + 1;
                    _lateFailCount[h] = count;
                    Logger.LogError($"[LateUpdate] {h.GetType().Name}.OnLateUpdate 异常 (连续第 {count} 次): {ex.Message}\n{ex.StackTrace}");
                    if (MaxConsecutiveFailuresBeforeDisable > 0 && count >= MaxConsecutiveFailuresBeforeDisable)
                    {
                        Unregister(h);
                        Logger.LogWarning($"[LateUpdate] 已自动移除 {h.GetType().Name}");
                    }
                }
            }
        }
    }
}
