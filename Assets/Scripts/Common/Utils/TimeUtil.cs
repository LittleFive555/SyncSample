using System;

namespace SyncSample.Common
{
    /// <summary>
    /// 时间戳工具，前后端共用（.NET Standard 2.0 兼容）
    /// </summary>
    public static class TimeUtil
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long UtcNowMillis()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }
    }
}
