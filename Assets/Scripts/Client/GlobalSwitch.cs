using UnityEngine;

namespace SyncSample.Client
{
    public class GlobalSwitch : MonoBehaviour
    {
        public static GlobalSwitch Instance;

        [SerializeField, Header("添加发送延迟(ms)")]
        public int AddSendDelay = 0;
        [SerializeField, Header("添加接收延迟(ms)")]
        public int AddReceiveDelay = 0;

        private void Awake()
        {
            Instance = this;
        }
    }
}