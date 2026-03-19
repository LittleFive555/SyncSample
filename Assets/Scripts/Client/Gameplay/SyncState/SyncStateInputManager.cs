using System;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 状态同步模式：采集本机轴向输入，按服务器逻辑帧间隔发往服务器（不驱动本地世界推演）。
    /// </summary>
    public class SyncStateInputManager : MonoBehaviour
    {
        [SerializeField] private TcpGameClient client;

        private float _dx;
        private float _dy;
        private float _accumulatedSendTime;

        /// <summary> 下一条发往服务器的输入所带的逻辑帧号，单调递增，避免同一帧号重复发送。 </summary>
        private long _nextOutgoingFrame;
        private long _lastServerFrame = -1;

        public static void Initialize()
        {
            var obj = new GameObject("SyncStateInputManager");
            obj.AddComponent<SyncStateInputManager>();
            DontDestroyOnLoad(obj);
        }

        private void Awake()
        {
            if (client == null)
                client = FindObjectOfType<TcpGameClient>();
        }

        private void Update()
        {
            _dx = Input.GetAxisRaw("Horizontal");
            _dy = Input.GetAxisRaw("Vertical");
            SendInput();
        }

        private void SendInput()
        {
            if (client == null || !client.IsConnected)
                return;
            var world = SyncStateWorldManager.Instance;
            if (!world.HasWorldStateSynced)
            {
                _nextOutgoingFrame = 0;
                _lastServerFrame = -1;
                _accumulatedSendTime = 0f;
                return;
            }

            float step = world.FrameDeltaTime;
            if (step <= 0f)
                step = 0.05f;

            // 快照帧前进时，保证待发帧至少为「服务器当前帧 + 1」，避免落后于权威帧号
            long cf = world.CurrentFrame;
            if (cf > _lastServerFrame)
            {
                _lastServerFrame = cf;
                _nextOutgoingFrame = Math.Max(_nextOutgoingFrame, cf + 1);
            }

            _accumulatedSendTime += Time.deltaTime;
            while (_accumulatedSendTime >= step)
            {
                _accumulatedSendTime -= step;
                SendCurrentInput();
            }
        }


        private void SendCurrentInput()
        {
            long frame = _nextOutgoingFrame++;
            var msg = new PlayerInputMessage(frame, FixedPoint.FromFloat(_dx), FixedPoint.FromFloat(_dy));

            string json = JsonUtility.ToJson(msg);
            int sendDelayMs = GlobalSwitch.Instance != null ? GlobalSwitch.Instance.AddSendDelay : 0;
            if (sendDelayMs > 0)
            {
                var c = client;
                GameLooper.Instance.RunAfterDelayMilliseconds(sendDelayMs, () =>
                {
                    if (c != null && c.IsConnected)
                        c.Send(MessageType.PlayerInput, json);
                });
            }
            else
                client.Send(MessageType.PlayerInput, json);
        }
    }
}
