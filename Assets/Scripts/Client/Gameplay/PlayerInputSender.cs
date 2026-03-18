using System.Threading.Tasks;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 采集上下左右输入，在 WorldManager 推进逻辑帧之后由 Dispatcher 驱动，每逻辑帧仅发送一次（含空操作）。
    /// </summary>
    public class PlayerInputSender : MonoBehaviour
    {
        [SerializeField] private TcpGameClient client;

        private float _dx;
        private float _dy;
        private long _lastSentFrame = -1;

        private void Awake()
        {
            if (client == null) client = FindObjectOfType<TcpGameClient>();
        }

        private void Update()
        {
            _dx = Input.GetAxisRaw("Horizontal");
            _dy = Input.GetAxisRaw("Vertical");
        }

        private void FixedUpdate()
        {
            if (client == null || !client.IsConnected) return;
            if (!PlayerInputSync.AllClientsConnected()) return;

            long currentFrame = WorldManager.Instance.CurrentFrame;
            if (currentFrame <= _lastSentFrame)
                return;

            _lastSentFrame = currentFrame;
            long frame = currentFrame + 1;
            var msg = new PlayerInputMessage(frame, FixedPoint.FromFloat(_dx), FixedPoint.FromFloat(_dy));
            if (GlobalSwitch.Instance.AddSendDelay > 0)
            {
                Task.Run(async () => {
                    await Task.Delay(GlobalSwitch.Instance.AddSendDelay);
                    client.Send(MessageType.PlayerInput, JsonUtility.ToJson(msg));
                });
            }
            else
            {
                client.Send(MessageType.PlayerInput, JsonUtility.ToJson(msg));
            }
        }
    }
}
