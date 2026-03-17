using System;

namespace SyncSample.Common
{
    /// <summary>
    /// 定点数：协议中数值用整数表示，实际值 = 整数 / Scale。
    /// 避免协议使用浮点数带来的平台差异与不确定性。
    /// </summary>
    [Serializable]
    public struct FixedPoint
    {
        /// <summary> 缩放因子：1 单位 = 实际 0.001。 </summary>
        public const int Scale = 1000;

        /// <summary> 原始整数值（序列化字段，实际值 = raw / Scale）。 </summary>
        public int raw;

        /// <summary> 原始整数值（用于代码中读写）。 </summary>
        public int RawValue => raw;

        public FixedPoint(int rawValue)
        {
            raw = rawValue;
        }

        /// <summary> 从浮点构造定点数（四舍五入）。 </summary>
        public static FixedPoint FromFloat(float value)
        {
            return new FixedPoint((int)Math.Round(value * Scale));
        }

        /// <summary> 转为浮点。 </summary>
        public float ToFloat()
        {
            return raw / (float)Scale;
        }
    }
}
