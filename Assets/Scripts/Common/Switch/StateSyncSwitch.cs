using UnityEngine;

namespace SyncSample.Common
{
    public class StateSyncSwitch : MonoBehaviour
    {
        [SerializeField, Header("服务器帧间隔(s)")]
        public float FrameDeltaTime = 0.05f;

        [SerializeField, Header("是否启用客户端预测")]
        public bool ClientPrediction = false;

        [SerializeField, Header("客户端时钟每帧最大纠偏比例")]
        public float ClockCorrectionRate = 0.2f;

        [SerializeField, Header("超过多少帧误差时直接校时")]
        public float SnapThresholdInFrames = 2f;
    }
}
