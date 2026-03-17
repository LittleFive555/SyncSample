namespace SyncSample.Common
{
    /// <summary>
    /// 消息类型常量，前后端共用
    /// </summary>
    public static class MessageType
    {
        public const string Ping = "ping";
        public const string Pong = "pong";
        public const string Echo = "echo";
        public const string Chat = "chat";
        public const string Error = "error";

        /// <summary> 连接协议：客户端发送自己的名字 </summary>
        public const string Join = "join";
        /// <summary> 服务器回复：当前所有已连接客户端信息 </summary>
        public const string ClientList = "client_list";
        /// <summary> 服务器广播：有新客户端加入，payload 为单条 ClientInfo </summary>
        public const string ClientJoined = "client_joined";

        /// <summary> 玩家输入：客户端发给服务器（仅 dx,dy,frame），服务器广播给所有人时带上 clientId </summary>
        public const string PlayerInput = "player_input";
    }
}
