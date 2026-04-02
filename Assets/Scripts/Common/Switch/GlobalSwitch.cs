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

        private LockstepSwitch _lockstepSwitch;
        public LockstepSwitch LockstepSwitch 
        {
            get
            {
                if (_lockstepSwitch == null)
                    _lockstepSwitch = GetComponent<LockstepSwitch>();
                return _lockstepSwitch;
            }
        }
        private StateSyncSwitch _stateSyncSwitch;
        public StateSyncSwitch StateSyncSwitch
        {
            get
            {
                if (_stateSyncSwitch == null)
                    _stateSyncSwitch = GetComponent<StateSyncSwitch>();
                return _stateSyncSwitch;
            }
        }
    }
}
