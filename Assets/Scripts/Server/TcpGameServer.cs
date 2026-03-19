using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SyncSample.Common;
using SyncSample.Server.Gameplay;

namespace SyncSample.Server
{
    /// <summary>
    /// 简单 TCP 游戏服务器：监听端口，接受连接，双向 JSON 消息收发。
    /// 可在 Unity Editor 或 Headless 构建中运行。
    /// </summary>
    public class TcpGameServer
    {
        private TcpListener _listener;
        private readonly int _port;
        private readonly List<ClientSession> _clients = new List<ClientSession>();
        private readonly object _clientsLock = new object();
        private Thread _acceptThread;
        private volatile bool _running;

        public int Port => _port;
        public bool IsRunning => _running;

        /// <summary> 收到任意客户端的消息时触发（主线程外） </summary>
        public Action<ClientSession, NetworkEnvelope> OnMessageReceived;

        /// <summary> 客户端连接时触发 </summary>
        public Action<ClientSession> OnClientConnected;

        /// <summary> 客户端断开时触发 </summary>
        public Action<ClientSession> OnClientDisconnected;

        public TcpGameServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            if (_running) return;
            _running = true;
            if (!GlobalSwitch.Instance.UseLockstep)
                StateSyncWorldManager.Instance.Start(this);

            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _acceptThread = new Thread(AcceptLoop) { IsBackground = true };
            _acceptThread.Start();
            Logger.Log($"监听端口 {_port}");
        }

        public void Stop()
        {
            _running = false;
            try { _listener?.Stop(); } catch { }
            lock (_clientsLock)
            {
                foreach (var c in _clients.ToArray())
                    c.Close();
                _clients.Clear();
            }
            Logger.Log("已停止");
        }

        private void AcceptLoop()
        {
            while (_running && _listener != null)
            {
                try
                {
                    var client = _listener.AcceptTcpClient();
                    var session = new ClientSession(client, this);
                    lock (_clientsLock) { _clients.Add(session); }
                    OnClientConnected?.Invoke(session);
                    session.BeginReceive();
                }
                catch (Exception e)
                {
                    if (_running)
                        Logger.LogWarning("Accept 异常: " + e.Message);
                }
            }
        }

        internal void RemoveClient(ClientSession session)
        {
            lock (_clientsLock) { _clients.Remove(session); }
            OnClientDisconnected?.Invoke(session);
        }

        /// <summary> 向所有已连接客户端广播一条消息 </summary>
        public void Broadcast(string type, string payloadJson)
        {
            byte[] packet = ProtocolHelper.Encode(type, payloadJson);
            lock (_clientsLock)
            {
                foreach (var c in _clients)
                    c.Send(packet);
            }
        }

        /// <summary> 向除 exclude 以外的所有已连接客户端发送一条消息 </summary>
        public void BroadcastExcept(ClientSession exclude, string type, string payloadJson)
        {
            if (exclude == null) { Broadcast(type, payloadJson); return; }
            byte[] packet = ProtocolHelper.Encode(type, payloadJson);
            lock (_clientsLock)
            {
                foreach (var c in _clients)
                {
                    if (c == exclude) continue;
                    c.Send(packet);
                }
            }
        }

        /// <summary> 获取当前所有会话的快照（Id + Name），用于组 ClientList 回复。 </summary>
        public ClientSession[] GetSessionsSnapshot()
        {
            lock (_clientsLock)
            {
                return _clients.ToArray();
            }
        }
    }
}
