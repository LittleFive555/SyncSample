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
                    _instance = FindObjectOfType<GlobalSwitch>();
                return _instance;
            }
        }

        [SerializeField, Header("使用锁步/状态同步")]
        public bool UseLockstep = false;
        [SerializeField, Header("添加发送延迟(ms)")]
        public int AddSendDelay = 0;
        [SerializeField, Header("添加接收延迟(ms)")]
        public int AddReceiveDelay = 0;

        [Header("锁步同步：")]
        [SerializeField, Header("预期客户端数量")]
        public int ExpectedClientCount = 2;
        [SerializeField, Header("逻辑帧间隔(s)")]
        public float LogicFixedDeltaTime = 0.03333333f;
        [SerializeField, Header("是否启用插值")]
        public bool LockstepInterpolation = false;
        [SerializeField, Header("是否启用乐观锁步")]
        public bool OptimisticLockstep = false;
    }
}
