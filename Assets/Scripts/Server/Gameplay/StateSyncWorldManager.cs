using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SyncSample.Common;
using SyncSample.Common.Model;
using UnityEngine;

namespace SyncSample.Server.Gameplay
{
    public class StateSyncWorldManager
    {
        private static StateSyncWorldManager _instance;
        public static StateSyncWorldManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StateSyncWorldManager();
                return _instance;
            }
        }

        /// <summary>已推进的逻辑时间（毫秒），约等于 World 时间轴 </summary>
        private long _currentFrame;
        private long _accumulatedLogicTimeMs;
        private Thread _updateThread;

        private SortedList<long, List<PlayerInputMessage>> _playerInputs = new SortedList<long, List<PlayerInputMessage>>();
        private Dictionary<string, MsgCharacterEntity> _characters = new Dictionary<string, MsgCharacterEntity>();
        private TcpGameServer _server;

        public void Start(TcpGameServer server)
        {
            _server = server;
            _accumulatedLogicTimeMs = 0;
            _currentFrame = 0;
            _updateThread = new Thread(UpdateLoop) { IsBackground = true };
            _updateThread.Start();
        }

        /// <summary> 与客户端 Character.moveSpeed 一致，输入为轴向 -1..1 时换算成速度（单位/秒）。 </summary>
        private const float InputMoveSpeed = 3f;

        private void Update(long frame, float deltaTime)
        {
            ApplyPlayerInputsForFrame(frame);

            foreach (var character in _characters.Values)
            {
                character.x += character.dx * deltaTime;
                character.y += character.dy * deltaTime;
            }
            var worldState = new WorldStateMessage
            {
                frame = frame,
                frameDeltaTime = deltaTime,
                characters = GetAllCharactersSnapshot()
            };
            _server.Broadcast(MessageType.WorldState, JsonUtility.ToJson(worldState));
        }

        public MsgCharacterEntity AddCharacter(string id, string name)
        {
            var character = new MsgCharacterEntity
            {
                id = id,
                name = name,
                x = 0,
                y = 0,
                dx = 0,
                dy = 0
            };
            _characters[id] = character;
            return character;
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
            foreach (var c in _characters.Values)
            {
                c.dx = 0f;
                c.dy = 0f;
            }

            lock (_playerInputs)
            {
                if (!_playerInputs.TryGetValue(frame, out var list) || list == null || list.Count == 0)
                    return;

                for (int i = 0; i < list.Count; i++)
                {
                    var input = list[i];
                    if (input == null || string.IsNullOrEmpty(input.clientId))
                        continue;
                    if (!_characters.TryGetValue(input.clientId, out var ch))
                        continue;
                    ch.dx = input.input.GetHorizontal() * InputMoveSpeed;
                    ch.dy = input.input.GetVertical() * InputMoveSpeed;
                }

                _playerInputs.Remove(frame);
            }
        }

        /// <summary> 构建当前世界快照（拷贝），用于 WorldState 协议。 </summary>
        public MsgCharacterEntity[] GetAllCharactersSnapshot()
        {
            var arr = new MsgCharacterEntity[_characters.Count];
            int i = 0;
            foreach (var c in _characters.Values)
            {
                arr[i++] = new MsgCharacterEntity
                {
                    id = c.id,
                    name = c.name,
                    x = c.x,
                    y = c.y,
                    dx = c.dx,
                    dy = c.dy
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