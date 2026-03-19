using SyncSample.Common;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 状态同步世界：仅在收到服务器的 WorldState 时更新世界与帧号，不在 Update 中做本地推演。
    /// </summary>
    public class SyncStateWorldManager : IUpdatable
    {
        private static SyncStateWorldManager _instance;
        public static SyncStateWorldManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SyncStateWorldManager();
                return _instance;
            }
        }

        private long _currentFrame;
        private float _frameDeltaTime;
        private bool _hasReceivedFirstWorldState;

        /// <summary> 与服务器最近一次快照对应的逻辑帧号。 </summary>
        public long CurrentFrame => _currentFrame;

        /// <summary> 服务器逻辑帧时长（最近一次 WorldState）。 </summary>
        public float FrameDeltaTime => _frameDeltaTime;

        /// <summary> 是否已收到过至少一条 WorldState（可开始按服务器节奏发输入等）。 </summary>
        public bool HasWorldStateSynced => _hasReceivedFirstWorldState;

        public void Initialize()
        {
            _hasReceivedFirstWorldState = false;
            _currentFrame = 0;
            _frameDeltaTime = 0f;
        }

        /// <summary> 断线重连等场景下重置，使下一条 WorldState 再次走首次初始化。 </summary>
        public void ResetSession()
        {
            _hasReceivedFirstWorldState = false;
            _currentFrame = 0;
            _frameDeltaTime = 0f;
        }

        /// <summary> 由网络层在收到 WorldState 时调用：首次包做初始化并同步帧参数，此后每次应用快照并更新帧号。 </summary>
        public void UpdateWorldState(WorldStateMessage state)
        {
            if (state == null)
                return;

            if (!_hasReceivedFirstWorldState)
            {
                _hasReceivedFirstWorldState = true;
                _frameDeltaTime = state.frameDeltaTime;
                Logger.Log($"[SyncStateWorld] 首次 WorldState，初始化并同步服务器帧号 frame={state.frame}, frameDeltaTime={state.frameDeltaTime}");
            }

            _currentFrame = state.frame;

            var cm = CharacterManager.Instance;
            if (cm != null)
                cm.ApplyServerWorldState(state.characters);
        }

        public void OnUpdate(float deltaTime)
        {
            // 状态同步：世界只在 UpdateWorldState（收到服务器消息）中更新，此处不做时间累积或本地模拟。
        }
    }
}