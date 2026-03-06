using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using SyncSample.Common;

namespace SyncSample.Server
{
    /// <summary>
    /// 单个客户端连接会话：专用接收线程 + 长度前缀 JSON 包。
    /// </summary>
    public class ClientSession
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly TcpGameServer _server;
        private readonly byte[] _readBuffer = new byte[64 * 1024];
        private readonly List<byte> _incoming = new List<byte>();
        private readonly object _incomingLock = new object();
        private readonly object _sendLock = new object();
        private Thread _receiveThread;
        private volatile bool _closed;

        public string Id { get; }
        public TcpClient TcpClient => _client;

        public ClientSession(TcpClient client, TcpGameServer server)
        {
            _client = client;
            _stream = client.GetStream();
            _server = server;
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        /// <summary> 启动专用接收线程 </summary>
        public void BeginReceive()
        {
            _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            _receiveThread.Start();
        }

        private void ReceiveLoop()
        {
            Logger.Log($"ClientSession {Id} 开始接收数据");
            while (!_closed && _stream != null)
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
                ProcessIncoming();
            }
            if (!_closed)
                Close();
        }

        private void ProcessIncoming()
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
                try
                {
                    _server.OnMessageReceived?.Invoke(this, envelope);
                }
                catch (Exception e)
                {
                    Logger.LogWarning("处理消息异常: " + e.Message);
                }
            }
            lock (_incomingLock)
            {
                if (offset > 0)
                    _incoming.RemoveRange(0, offset);
            }
        }

        /// <summary> 发送已编码的包 </summary>
        internal void Send(byte[] packet)
        {
            if (_closed) return;
            lock (_sendLock)
            {
                try
                {
                    _stream.Write(packet, 0, packet.Length);
                }
                catch
                {
                    Close();
                }
            }
        }

        /// <summary> 发送一条 JSON 消息 </summary>
        public void Send(string type, string payloadJson)
        {
            Send(ProtocolHelper.Encode(type, payloadJson));
        }

        public void Close()
        {
            if (_closed) return;
            _closed = true;
            try { _stream?.Close(); _client?.Close(); } catch { }
            _server.RemoveClient(this);
        }
    }
}
