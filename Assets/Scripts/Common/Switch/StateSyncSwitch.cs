using UnityEngine;

namespace SyncSample.Common
{
    public class StateSyncSwitch : MonoBehaviour
    {
        [SerializeField, Header("服务器帧间隔(s)")]
        public float FrameDeltaTime = 0.05f;

        [SerializeField, Header("是否启用客户端预测")]
        public bool ClientPrediction = false;
    }
}
