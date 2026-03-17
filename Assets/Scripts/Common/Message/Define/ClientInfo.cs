using System;

namespace SyncSample.Common
{
    /// <summary>
    /// 单个客户端信息，用于 ClientList 与 ClientJoined。
    /// </summary>
    [Serializable]
    public class ClientInfo
    {
        public string id;
        public string name;

        public ClientInfo() { }

        public ClientInfo(string id, string name)
        {
            this.id = id ?? string.Empty;
            this.name = name ?? string.Empty;
        }
    }
}
