using System;
using SyncSample.Client.Airplane;
using SyncSample.Client.Airplane.Logic;
using SyncSample.Client.Airplane.View;
using SyncSample.Client.Gameplay;
using SyncSample.Client.Gameplay.Lockstep;
using SyncSample.Client.Gameplay.Lockstep.World.Logic;
using SyncSample.Client.Gameplay.StateSync;
using SyncSample.Client.Gameplay.StateSync.World.Logic;
using SyncSample.Client.Gameplay.World.View;
using SyncSample.Client.Race;
using SyncSample.Client.Race.Logic;
using SyncSample.Client.Race.View;
using SyncSample.Client.UI;
using SyncSample.Common;
using UnityEngine;

using LockstepCharacterManager = SyncSample.Client.Gameplay.Lockstep.World.Logic.CharacterManager;
using SyncStateCharacterManager = SyncSample.Client.Gameplay.StateSync.World.Logic.CharacterManager;

namespace SyncSample.Client
{

    /// <summary>
    /// 客户端示例：连接后发 Ping、处理 Pong/Echo/Chat。挂到场景中并指定 TcpGameClient 引用。
    /// </summary>
    public class GameMain : MonoBehaviour
    {
        [SerializeField, Header("服务器地址")]
        public string ServerAddress = "127.0.0.1";

        [SerializeField, Header("服务器端口")]
        public int ServerPort = 8888;

        [SerializeField, Header("Ping间隔")]
        public float PingInterval = 2f;

        public static GameMain Instance { get; private set; }

        [NonSerialized]
        public TcpGameClient Client;

        [NonSerialized]
        public GameLooper GameLooper;

        private float _nextPing;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Launch();
        }

        private void Update()
        {
            if (Client == null || !Client.IsConnected) return;
            if (Time.time >= _nextPing)
            {
                _nextPing = Time.time + PingInterval;
                var ping = new PingMessage(TimeUtil.UtcNowMillis());
                Client.Send(MessageType.Ping, JsonUtility.ToJson(ping));
            }
        }

        private void OnDestroy()
        {
            DisconnectServer();
        }


        public void Launch()
        {
            Logger.Log("Before Launch");

            // 初始化单例
            GameLooper = new GameObject("GameLooper").AddComponent<GameLooper>();
            GameLooper.transform.SetParent(transform);
            InputManager.Initialize();
            if (GlobalSwitch.Instance.SyncMode == SyncMode.Lockstep)
            {
                LockstepWorldManager.Instance.Initialize();
                
                LockstepCharacterManager.Instance.OnPlayerCreated += CharacterSpawner.Instance.EnsurePlayer;
                LockstepCharacterManager.Instance.OnPlayerRemoved += CharacterSpawner.Instance.RemovePlayer;

                GameLooper.Updater.Register(LockstepWorldManager.Instance);
                LockstepPlayerInputSync.SetExpectedClientCount(GlobalSwitch.Instance.LockstepSwitch.ExpectedClientCount);
            }
            else if (GlobalSwitch.Instance.SyncMode == SyncMode.StateSync)
            {
                SyncStateWorldManager.Instance.Initialize();

                SyncStateCharacterManager.Instance.OnPlayerCreated += CharacterSpawner.Instance.EnsurePlayer;
                SyncStateCharacterManager.Instance.OnPlayerRemoved += CharacterSpawner.Instance.RemovePlayer;

                GameLooper.Updater.Register(SyncStateWorldManager.Instance);
            }
            else if (GlobalSwitch.Instance.SyncMode == SyncMode.Race_StateSync)
            {
                RaceWorldManager.Instance.Initialize();
                
                VehicleManager.Instance.OnVehicleCreated += VehicleSpawner.Instance.EnsureVehicle;
                VehicleManager.Instance.OnVehicleRemoved += VehicleSpawner.Instance.RemoveVehicle;

                GameLooper.Updater.Register(RaceWorldManager.Instance);
            }
            else if (GlobalSwitch.Instance.SyncMode == SyncMode.Airplane_Lockstep)
            {
                AirplaneWorldManager.Instance.Initialize();
                
                AirplaneManager.Instance.OnPlayerCreated += Spawner.Instance.EnsurePlayer;
                AirplaneManager.Instance.OnPlayerRemoved += Spawner.Instance.RemovePlayer;
                AirplaneManager.Instance.OnEnemyCreated += Spawner.Instance.EnsureEnemy;
                AirplaneManager.Instance.OnEnemyRemoved += Spawner.Instance.RemoveEnemy;
                AirplaneManager.Instance.OnBulletCreated += Spawner.Instance.EnsureBullet;
                AirplaneManager.Instance.OnBulletRemoved += Spawner.Instance.RemoveBullet;
                
                GameLooper.Updater.Register(AirplaneWorldManager.Instance);
                AirplanePlayerInputSync.SetExpectedClientCount(GlobalSwitch.Instance.LockstepSwitch.ExpectedClientCount);
            }

            Logger.Log("After Launch");
        }

        public void ConnectServer()
        {
            Client = new GameObject("TcpGameClient").AddComponent<TcpGameClient>();
            Client.transform.SetParent(transform);
            Client.Connect(ServerAddress, ServerPort);

            Client.OnConnected += OnConnected;
            Client.OnDisconnected += OnDisconnected;
            Client.OnMessageReceived += OnMessageReceived;
            if (GlobalSwitch.Instance.SyncMode == SyncMode.Lockstep)
                Client.OnMessageReceived += LockstepMessageHandlers.OnMessageReceived;
            else if (GlobalSwitch.Instance.SyncMode == SyncMode.StateSync)
                Client.OnMessageReceived += SyncStateMessageHandlers.OnMessageReceived;
            else if (GlobalSwitch.Instance.SyncMode == SyncMode.Race_StateSync)
                Client.OnMessageReceived += RaceMessageHandlers.OnMessageReceived;
            else if (GlobalSwitch.Instance.SyncMode == SyncMode.Airplane_Lockstep)
                Client.OnMessageReceived += AirplaneMessageHandlers.OnMessageReceived;
        }

        public void DisconnectServer()
        {
            if (Client == null) return;
            Client.OnConnected -= OnConnected;
            Client.OnDisconnected -= OnDisconnected;
            Client.OnMessageReceived -= OnMessageReceived;
            if (GlobalSwitch.Instance.SyncMode == SyncMode.Lockstep)
                Client.OnMessageReceived -= LockstepMessageHandlers.OnMessageReceived;
            else if (GlobalSwitch.Instance.SyncMode == SyncMode.StateSync)
                Client.OnMessageReceived -= SyncStateMessageHandlers.OnMessageReceived;
            else if (GlobalSwitch.Instance.SyncMode == SyncMode.Race_StateSync)
                Client.OnMessageReceived -= RaceMessageHandlers.OnMessageReceived;
            else if (GlobalSwitch.Instance.SyncMode == SyncMode.Airplane_Lockstep)
                Client.OnMessageReceived -= AirplaneMessageHandlers.OnMessageReceived;
        }

        private void OnConnected()
        {
            var join = new JoinMessage("Player");
            Client.Send(MessageType.Join, JsonUtility.ToJson(join));
            Logger.Log("已连接，已发送名字: " + join.name);
        }

        private void OnDisconnected()
        {
            Logger.Log("已断开");

            if (GlobalSwitch.Instance.SyncMode == SyncMode.StateSync)
                SyncStateWorldManager.Instance.ResetSession();
            else if (GlobalSwitch.Instance.SyncMode == SyncMode.Race_StateSync)
                RaceWorldManager.Instance.ResetSession();
        }

        private void OnMessageReceived(NetworkEnvelope envelope)
        {
            if (string.IsNullOrEmpty(envelope?.type)) return;
            switch (envelope.type)
            {
                case MessageType.Pong:
                    try
                    {
                        var pong = JsonUtility.FromJson<PongMessage>(envelope.payload);
                        long rtt = TimeUtil.UtcNowMillis() - pong.timestamp;
                        UIInfo.Instance.SetDelay(rtt);
                        if (GlobalSwitch.Instance.SyncMode == SyncMode.Race_StateSync)
                            RaceWorldManager.Instance.UpdateRtt(rtt);
                    }
                    catch { }
                    break;
                case MessageType.Echo:
                    Logger.Log("Echo: " + envelope.payload);
                    break;
                case MessageType.Chat:
                    try
                    {
                        var chat = JsonUtility.FromJson<ChatMessage>(envelope.payload);
                        Logger.Log($"聊天 [{chat.sender}]: {chat.text}");
                    }
                    catch { }
                    break;
                case MessageType.ClientList:
                    try
                    {
                        var list = JsonUtility.FromJson<ClientListMessage>(envelope.payload);
                        if (list.clients != null)
                        {
                            Logger.Log($"当前在线 {list.clients.Length} 人");
                            for (int i = 0; i < list.clients.Length; i++)
                                Logger.Log($"  - [{list.clients[i].id}] {list.clients[i].name}");
                        }
                    }
                    catch { }
                    break;
                case MessageType.ClientJoined:
                    try
                    {
                        var info = JsonUtility.FromJson<ClientInfo>(envelope.payload);
                        Logger.Log($"新玩家加入: [{info.id}] {info.name}");
                    }
                    catch { }
                    break;
            }
        }

        /// <summary> 发送 Echo（可由 UI 按钮调用） </summary>
        public void SendEcho(string content)
        {
            if (Client == null || !Client.IsConnected) return;
            Client.Send(MessageType.Echo, JsonUtility.ToJson(new EchoMessage(content)));
        }

        /// <summary> 发送聊天（可由 UI 按钮调用） </summary>
        public void SendChat(string sender, string text)
        {
            if (Client == null || !Client.IsConnected) return;
            Client.Send(MessageType.Chat, JsonUtility.ToJson(new ChatMessage(sender, text)));
        }
    }
}
