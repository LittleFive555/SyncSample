using System;
using SyncSample.Common.Model.Race;

namespace SyncSample.Common
{
    /// <summary>
    /// 竞速世界状态：Join 成功后由服务器下发，包含当前所有车辆实体。
    /// </summary>
    [Serializable]
    public class RaceWorldStateMessage
    {
        /// <summary>
        /// 逻辑帧号
        /// </summary>
        public long frame;
        /// <summary>
        /// 逻辑帧时间
        /// </summary>
        public float frameDeltaTime;
        /// <summary>
        /// 车辆实体列表
        /// </summary>
        public VehicleEntity[] vehicles;

        public RaceWorldStateMessage() { }

        public RaceWorldStateMessage(VehicleEntity[] vehicles)
        {
            this.vehicles = vehicles ?? Array.Empty<VehicleEntity>();
        }
    }
}
