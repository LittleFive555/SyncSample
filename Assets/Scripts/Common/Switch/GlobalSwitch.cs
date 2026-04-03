using UnityEngine;

namespace SyncSample.Common
{
    public enum SyncMode
    {
        Lockstep,
        StateSync,
    }

    public class GlobalSwitch : MonoBehaviour
    {
        private static GlobalSwitch _instance;
        public static GlobalSwitch Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<GlobalSwitch>();
                return _instance;
            }
        }

        [SerializeField, Header("同步模式")]
        public SyncMode SyncMode = SyncMode.Lockstep;
        [SerializeField, Header("添加发送延迟(ms)")]
        public int AddSendDelay = 0;
        [SerializeField, Header("添加接收延迟(ms)")]
        public int AddReceiveDelay = 0;
        [SerializeField, Header("是否启用客户端插值")]
        public bool ClientInterpolation = false;

        [Header("绑定")]
        [SerializeField]
        private LockstepSwitch _lockstepSwitch;
        public LockstepSwitch LockstepSwitch => _lockstepSwitch;

        [SerializeField]
        private StateSyncSwitch _stateSyncSwitch;
        public StateSyncSwitch StateSyncSwitch => _stateSyncSwitch;
    }
}
