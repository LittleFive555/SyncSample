using UnityEngine;

namespace SyncSample.Common
{
    public class GlobalSwitch : MonoBehaviour
    {
        private static GlobalSwitch _instance;
        public static GlobalSwitch Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GlobalSwitch>();
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        [SerializeField, Header("添加发送延迟(ms)")]
        public int AddSendDelay = 0;
        [SerializeField, Header("添加接收延迟(ms)")]
        public int AddReceiveDelay = 0;
        [SerializeField, Header("使用锁步(Lockstep)")]
        public bool UseLockstep = false;
    }
}