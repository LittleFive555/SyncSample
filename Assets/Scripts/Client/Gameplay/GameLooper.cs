using System;
using System.Collections;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    public class GameLooper : MonoBehaviour
    {
        /// <summary> 客户端 Update 调度器，各模块注册 IUpdatable/IFixedUpdatable/ILateUpdatable 后由本类统一驱动并做异常隔离。 </summary>
        public UpdateDispatcher Updater { get; } = new UpdateDispatcher();

        /// <summary>
        /// 在主线程上延迟执行（模拟网络延迟）。发送/入队等涉及 Unity 或网络 API 的逻辑应通过此方法调度，勿用 Task.Run。
        /// </summary>
        public void RunAfterDelayMilliseconds(int delayMs, Action action)
        {
            if (action == null) return;
            if (delayMs <= 0)
            {
                action();
                return;
            }
            StartCoroutine(RunAfterDelayCoroutine(delayMs / 1000f, action));
        }

        private static IEnumerator RunAfterDelayCoroutine(float seconds, Action action)
        {
            if (seconds > 0f)
                yield return new WaitForSecondsRealtime(seconds);
            action?.Invoke();
        }

        private void Update()
        {
            try
            {
                Updater?.DispatchUpdate(Time.deltaTime);
            }
            catch (Exception ex)
            {
                Logger.LogError($"[GameMain.Update] 调度器异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void FixedUpdate()
        {
            try
            {
                Updater?.DispatchFixedUpdate(Time.fixedDeltaTime);
            }
            catch (Exception ex)
            {
                Logger.LogError($"[GameMain.FixedUpdate] 调度器异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void LateUpdate()
        {
            try
            {
                Updater?.DispatchLateUpdate(Time.deltaTime);
            }
            catch (Exception ex)
            {
                Logger.LogError($"[GameMain.LateUpdate] 调度器异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnDestroy()
        {
            Logger.Log("GameMain OnDestroy");
        }
    }
}