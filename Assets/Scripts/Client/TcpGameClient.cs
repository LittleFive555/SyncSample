using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client
{
    /// <summary>
    /// Unity 端 TCP 客户端：连接服务器，双向 JSON 消息收发。
    /// 建议挂到 GameObject 上，在主线程中调用 Connect/Send，收到消息会在主线程回调。
    /// </summary>
    public class TcpGameClient : MonoBehaviour
    {
        [SerializeField] private string host = "127.0.0.1";
        [SerializeField] private int port = 8888;

        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _receiveThread;
        private readonly byte[] _readBuffer = new byte[64 * 1024];
        private readonly List<byte> _incoming = new List<byte>();
        private readonly object _incomingLock = new object();
        private readonly Queue<NetworkEnvelope> _mainThreadQueue = new Queue<NetworkEnvelope>();
        private readonly Queue<Action> _mainThreadActions = new Queue<Action>();
        private readonly object _queueLock = new object();
        private volatile bool _connected;
        private readonly object _sendLock = new object();

        /// <summary> 是否已连接 </summary>
        public bool IsConnected => _connected;

        /// <summary> 收到服务器消息时在主线程触发 </summary>
        public Action<NetworkEnvelope> OnMessageReceived;

        /// <summary> 连接成功时触发 </summary>
        public Action OnConnected;

        /// <summary> 连接断开时触发 </summary>
        public Action OnDisconnected;

        /// <summary> 连接服务器 </summary>
        public void Connect(string hostOverride = null, int portOverride = 0)
        {
            if (_connected) return;
            string h = string.IsNullOrEmpty(hostOverride) ? host : hostOverride;
            int p = portOverride > 0 ? portOverride : port;
            try
            {
                _client = new TcpClient();
                _client.Connect(h, p);
                _stream = _client.GetStream();
                _connected = true;
                _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                _receiveThread.Start();
                MainThreadInvoke(() => OnConnected?.Invoke());
                Logger.Log($"已连接 {h}:{p}");
            }
            catch (Exception e)
            {
                Logger.LogError("连接失败: " + e.Message);
            }
        }

        /// <summary> 断开连接 </summary>
        public void Disconnect()
        {
            _connected = false;
            try { _stream?.Close(); _client?.Close(); } catch { }
            _stream = null;
            _client = null;
            MainThreadInvoke(() => OnDisconnected?.Invoke());
            Logger.Log("已断开");
        }

        private void ReceiveLoop()
        {
            while (_connected && _stream != null)
            {
                int count;
                try
                {
                    count = _stream.Read(_readBuffer, 0, _readBuffer.Length);
                }
                catch
                {
                    break;
                }
                if (count <= 0) break;
                lock (_incomingLock)
                {
                    for (int i = 0; i < count; i++)
                        _incoming.Add(_readBuffer[i]);
                }
                DrainIncoming();
            }
            if (_connected)
            {
                _connected = false;
                MainThreadInvoke(() => OnDisconnected?.Invoke());
            }
        }

        private void DrainIncoming()
        {
            byte[] buf;
            int total;
            lock (_incomingLock)
            {
                total = _incoming.Count;
                if (total == 0) return;
                buf = _incoming.ToArray();
            }
            int offset = 0;
            while (offset < total)
            {
                var envelope = ProtocolHelper.TryDecode(buf, offset, total - offset, out int consumed);
                if (envelope == null) break;
                offset += consumed;
                lock (_queueLock) { _mainThreadQueue.Enqueue(envelope); }
            }
            lock (_incomingLock)
            {
                if (offset > 0) _incoming.RemoveRange(0, offset);
            }
        }

        private void MainThreadInvoke(Action action)
        {
            if (action == null) return;
            lock (_queueLock) { _mainThreadActions.Enqueue(action); }
        }

        private void Update()
        {
            lock (_queueLock)
            {
                while (_mainThreadActions.Count > 0)
                {
                    var action = _mainThreadActions.Dequeue();
                    try { action?.Invoke(); } catch (Exception e) { Logger.LogWarning(e.Message); }
                }
                while (_mainThreadQueue.Count > 0)
                {
                    var envelope = _mainThreadQueue.Dequeue();
                    try { OnMessageReceived?.Invoke(envelope); } catch (Exception e) { Logger.LogWarning(e.Message); }
                }
            }
        }

        /// <summary> 发送一条 JSON 消息 </summary>
        public void Send(string type, string payloadJson)
        {
            if (!_connected || _stream == null) return;
            byte[] packet = ProtocolHelper.Encode(type, payloadJson);
            lock (_sendLock)
            {
                try { _stream.Write(packet, 0, packet.Length); }
                catch { Disconnect(); }
            }
        }

        private void OnDestroy()
        {
            Disconnect();
        }
    }
}
