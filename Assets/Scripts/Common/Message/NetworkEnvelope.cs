using System;

namespace SyncSample.Common
{
    /// <summary>
    /// 网络消息外壳：类型 + JSON 载荷，用于 TCP 上的一条完整消息
    /// 每条消息格式：4 字节长度(小端) + UTF-8 JSON 字符串
    /// </summary>
    [Serializable]
    public class NetworkEnvelope
    {
        public string type;
        public string payload;

        public NetworkEnvelope() { }

        public NetworkEnvelope(string type, string payload)
        {
            this.type = type;
            this.payload = payload ?? string.Empty;
        }
    }
}
