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
    }
}
