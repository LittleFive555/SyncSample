using System.Linq;
using UnityEditor;
using UnityEngine;
using SyncSample.Server;
using SyncSample.Common;

namespace SyncSample.Server.Editor
{
    /// <summary>
    /// 通过菜单打开的服务器编辑器面板，不依赖场景中的 GameObject。
    /// 菜单：Tools -> Game Server
    /// </summary>
    public class GameServerEditorWindow : EditorWindow
    {
        private static TcpGameServer _server;
        private int _port = 8888;
        private Vector2 _scroll;
        private const int MaxLogLines = 50;

        [MenuItem("Tools/Game Server")]
        public static void Open()
        {
            var win = GetWindow<GameServerEditorWindow>("Game Server");
            win.minSize = new Vector2(280, 180);
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("TCP 游戏服务器", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            bool running = _server != null && _server.IsRunning;
            GUI.enabled = !running;
            _port = EditorGUILayout.IntField("端口", _port);
            GUI.enabled = true;

            EditorGUILayout.Space(6);

            if (!running)
            {
                if (GUILayout.Button("启动服务器", GUILayout.Height(28)))
                    StartServer();
            }
            else
            {
                EditorGUILayout.HelpBox($"正在监听 0.0.0.0:{_server.Port}\n客户端可连接 127.0.0.1:{_server.Port}", UnityEditor.MessageType.Info);
                if (GUILayout.Button("停止服务器", GUILayout.Height(28)))
                    StopServer();
            }

            EditorGUILayout.Space(8);

            EditorGUILayout.EndScrollView();
        }

        private void StartServer()
        {
            if (_server != null) return;
            _server = new TcpGameServer(_port);
            _server.OnClientConnected += OnClientConnected;
            _server.OnClientDisconnected += OnClientDisconnected;
            _server.OnMessageReceived += OnMessageReceived;
            _server.Start();
            Logger.Log($"[启动] 监听端口 {_port}");
        }

        private void StopServer()
        {
            if (_server == null) return;
            _server.Stop();
            _server = null;
            Logger.Log("[停止] 服务器已关闭");
        }

        private void OnClientConnected(ClientSession session)
        {
            Logger.Log($"[连接] 客户端 {session.Id}");
        }

        private void OnClientDisconnected(ClientSession session)
        {
            Logger.Log($"[断开] 客户端 {session.Id}");
        }

        private void OnMessageReceived(ClientSession session, NetworkEnvelope envelope)
        {
            GameServerMessageDispatcher.Handle(_server, session, envelope);
        }
    }
}
