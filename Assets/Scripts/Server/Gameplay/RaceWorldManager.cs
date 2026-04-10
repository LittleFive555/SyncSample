using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SyncSample.Common;
using SyncSample.Common.Model;
using SyncSample.Common.Model.Race;
using UnityEngine;

namespace SyncSample.Server.Gameplay
{
    public class RaceSyncWorldManager
    {
        private static RaceSyncWorldManager _instance;
        public static RaceSyncWorldManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RaceSyncWorldManager();
                return _instance;
            }
        }

        /// <summary>已推进的逻辑时间（毫秒），约等于 World 时间轴 </summary>
        private long _currentFrame;
        private long _accumulatedLogicTimeMs;
        private Thread _updateThread;

        private SortedList<long, List<PlayerInputMessage>> _playerInputs = new SortedList<long, List<PlayerInputMessage>>();
        private Dictionary<string, VehicleEntity> _vehicles = new Dictionary<string, VehicleEntity>();
        private TcpGameServer _server;

        public void Start(TcpGameServer server)
        {
            _server = server;
            _accumulatedLogicTimeMs = 0;
            _currentFrame = 0;
            _updateThread = new Thread(UpdateLoop) { IsBackground = true };
            _updateThread.Start();
        }

        private void Update(long frame, float deltaTime)
        {
            ApplyPlayerInputsForFrame(frame);

            foreach (var vehicle in _vehicles.Values)
            {
                vehicle.UpdateState(deltaTime);
                Logger.Log($"[{frame}]UpdateVehicle: {vehicle.id}, x: {vehicle.x}, z: {vehicle.z}, rotation: {vehicle.rotation}, speed: {vehicle.speed}");
            }

            var worldState = new RaceWorldStateMessage
            {
                frame = frame,
                frameDeltaTime = deltaTime,
                vehicles = GetAllVehiclesSnapshot()
            };
            _server.Broadcast(MessageType.RaceWorldState, JsonUtility.ToJson(worldState));
        }

        public VehicleEntity AddVehicle(string id, string name)
        {
            var vehicle = new VehicleEntity(id, name);
            _vehicles[id] = vehicle;
            return vehicle;
        }

        public void AppendPlayerInput(string clientId, PlayerInputMessage input)
        {
            lock (_playerInputs)
            {
                if (!_playerInputs.TryGetValue(input.frame, out var inputs))
                {
                    inputs = new List<PlayerInputMessage>();
                    _playerInputs[input.frame] = inputs;
                }
                inputs.Add(input);
            }
        }

        private void ApplyPlayerInputsForFrame(long frame)
        {
            lock (_playerInputs)
            {
                if (!_playerInputs.TryGetValue(frame, out var list) || list == null || list.Count == 0)
                    return;

                for (int i = 0; i < list.Count; i++)
                {
                    var input = list[i];
                    if (input == null || string.IsNullOrEmpty(input.clientId))
                        continue;
                    if (!_vehicles.TryGetValue(input.clientId, out var vehicle))
                        continue;
                    vehicle.ReceiveInput(input.input.GetHorizontal(), input.input.GetVertical());
                }

                _playerInputs.Remove(frame);
            }
        }

        /// <summary> 构建当前世界快照（拷贝），用于 RaceWorldState 协议。 </summary>
        public VehicleEntity[] GetAllVehiclesSnapshot()
        {
            var arr = new VehicleEntity[_vehicles.Count];
            int i = 0;
            foreach (var v in _vehicles.Values)
            {
                arr[i++] = new VehicleEntity(v.id, v.name)
                {
                    x = v.x,
                    z = v.z,
                    rotation = v.rotation,
                    speed = v.speed
                };
            }
            return arr;
        }

        /// <summary>
        /// 使用 Stopwatch 频率对齐系统高精度时钟，按固定 _frameTime 推进逻辑；
        /// 落后时在同一轮内连续补帧直到追上时钟，过快则睡到下一帧边界。
        /// </summary>
        private void UpdateLoop()
        {
            long frameTicks = (long)(GlobalSwitch.Instance.StateSyncSwitch.FrameDeltaTime * Stopwatch.Frequency);
            long nextFrameTick = Stopwatch.GetTimestamp();

            while (true)
            {
                long now = Stopwatch.GetTimestamp();

                while (now >= nextFrameTick)
                {
                    Update(_currentFrame, GlobalSwitch.Instance.StateSyncSwitch.FrameDeltaTime);
                    _currentFrame++;
                    _accumulatedLogicTimeMs += (long)(GlobalSwitch.Instance.StateSyncSwitch.FrameDeltaTime * 1000);
                    nextFrameTick += frameTicks;
                    now = Stopwatch.GetTimestamp();
                }

                now = Stopwatch.GetTimestamp();
                long remainingTicks = nextFrameTick - now;
                if (remainingTicks > 0)
                {
                    double sleepMs = remainingTicks * 1000.0 / Stopwatch.Frequency;
                    if (sleepMs >= 1.0)
                        Thread.Sleep((int)sleepMs);
                    else
                        Thread.Sleep(0); // 让出时间片，剩余由下一轮对齐
                }
                else
                    Thread.Sleep(0);
            }
        }
    }
}