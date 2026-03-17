using System;
using UnityEngine;
using SyncSample.Client;

namespace SyncSample.Client.Gameplay
{
    public class GameMain : MonoBehaviour
    {
        private static GameMain _instance;
        public static GameMain Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("GameMain").AddComponent<GameMain>();
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        /// <summary> 客户端 Update 调度器，各模块注册 IUpdatable/IFixedUpdatable/ILateUpdatable 后由本类统一驱动并做异常隔离。 </summary>
        public static UpdateDispatcher Dispatcher { get; private set; }

        private void Awake()
        {
            if (Dispatcher == null)
                Dispatcher = new UpdateDispatcher();
        }

        private void Start()
        {
            Logger.Log("GameMain Start");
            // 示例：注册需要每帧更新的模块（可改为从服务或配置获取）
            if (Dispatcher != null)
                Dispatcher.Register(new WorldManager());
        }

        private void Update()
        {
            try
            {
                Dispatcher?.DispatchUpdate(Time.deltaTime);
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
                Dispatcher?.DispatchFixedUpdate(Time.fixedDeltaTime);
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
                Dispatcher?.DispatchLateUpdate(Time.deltaTime);
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