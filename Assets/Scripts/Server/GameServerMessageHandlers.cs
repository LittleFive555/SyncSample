using SyncSample.Common;

namespace SyncSample.Server
{
    /// <summary>
    /// 服务器消息处理逻辑，供 GameServerRunner 与编辑器面板共用。
    /// </summary>
    public static class GameServerMessageDispatcher
    {
        private static bool _initialized = false;
        private static ISyncMessageHandler _syncMessageHandler;
        private static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            if (GlobalSwitch.Instance.UseLockstep)
                _syncMessageHandler = new LockstepMessageHandlers();
            else
                _syncMessageHandler = new StateSyncMessageHandlers();
        }
        public static void Handle(TcpGameServer server, ClientSession session, NetworkEnvelope envelope)
        {
            if (server == null || envelope == null || string.IsNullOrEmpty(envelope.type)) return;

            Initialize();

            switch (envelope.type)
            {
                case MessageType.Ping:
                    CommonMessageHandlers.HandlePing(session, envelope.payload);
                    break;
                case MessageType.Echo:
                    CommonMessageHandlers.HandleEcho(session, envelope.payload);
                    break;
                case MessageType.Chat:
                    CommonMessageHandlers.HandleChat(server, session, envelope.payload);
                    break;
                case MessageType.Join:
                    _syncMessageHandler.HandleJoin(server, session, envelope.payload);
                    break;
                case MessageType.PlayerInput:
                    _syncMessageHandler.HandlePlayerInput(server, session, envelope.payload);
                    break;
                default:
                    Logger.Log($"未知消息类型: {envelope.type}");
                    break;
            }
        }
    }
}
