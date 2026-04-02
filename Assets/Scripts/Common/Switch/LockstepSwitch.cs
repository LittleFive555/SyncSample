using UnityEngine;

namespace SyncSample.Common
{
    public class LockstepSwitch : MonoBehaviour
    {
        [SerializeField, Header("预期客户端数量")]
        public int ExpectedClientCount = 2;

        [SerializeField, Header("逻辑帧间隔(s)")]
        public float LogicDeltaTime = 0.03333333f;

        [SerializeField, Header("是否启用插值")]
        public bool Interpolation = false;
        
        [SerializeField, Header("是否启用乐观锁步")]
        public bool Optimistic = false;

    }
}
