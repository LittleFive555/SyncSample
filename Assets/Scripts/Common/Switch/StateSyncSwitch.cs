using UnityEngine;

namespace SyncSample.Common
{
    public class StateSyncSwitch : MonoBehaviour
    {
        [SerializeField, Header("帧间隔(s)")]
        public float FrameDeltaTime = 0.05f;

        [SerializeField, Header("延迟应用帧数")]
        public int DelayApplyFrameCount = 3;
    }
}
