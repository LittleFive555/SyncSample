using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace SyncSample.Common
{
    /// <summary>
    /// 协议编解码：长度(4字节小端) + UTF-8 JSON。前后端共用。
    /// </summary>
    public static class ProtocolHelper
    {
        private static readonly Encoding UTF8 = new UTF8Encoding(false);

        /// <summary>
        /// 将消息类型与 JSON 载荷编码为一条完整包（4 字节长度 + 正文）
        /// </summary>
        public static byte[] Encode(string type, string payloadJson)
        {
            var envelope = new NetworkEnvelope(type, payloadJson);
            string json = JsonUtility.ToJson(envelope);
            byte[] body = UTF8.GetBytes(json);
            byte[] len = BitConverter.GetBytes(body.Length);
            if (BitConverter.IsLittleEndian == false)
                Array.Reverse(len);
            var packet = new byte[4 + body.Length];
            Buffer.BlockCopy(len, 0, packet, 0, 4);
            Buffer.BlockCopy(body, 0, packet, 4, body.Length);
            return packet;
        }

        /// <summary>
        /// 从流中读取一条完整包并解析为 NetworkEnvelope；若不足一条则返回 null。
        /// </summary>
        public static NetworkEnvelope TryDecode(byte[] buffer, int offset, int count, out int consumed)
        {
            consumed = 0;
            if (count < 4) return null;
            int length = BitConverter.ToInt32(buffer, offset);
            if (length <= 0 || length > 1024 * 1024) // 限制单条 1MB
            {
                consumed = 4;
                return null;
            }
            if (count < 4 + length) return null;
            consumed = 4 + length;
            string json = UTF8.GetString(buffer, offset + 4, length);
            try
            {
                return JsonUtility.FromJson<NetworkEnvelope>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
