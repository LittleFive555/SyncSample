using System;

namespace SyncSample.Common
{
    /// <summary>
    /// 服务器回复的当前连接客户端列表（Join 协议回复）。
    /// </summary>
    [Serializable]
    public class ClientListMessage
    {
        public ClientInfo[] clients;

        public ClientListMessage() { }

        public ClientListMessage(ClientInfo[] clients)
        {
            this.clients = clients ?? new ClientInfo[0];
        }
    }
}
