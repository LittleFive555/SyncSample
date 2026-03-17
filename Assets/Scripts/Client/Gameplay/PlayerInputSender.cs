using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 采集上下左右输入，按帧发给服务器；位移在收到服务器广播后于 WorldManager.WaitForAllClientsThisFrame 中生效。
    /// </summary>
    public class PlayerInputSender : MonoBehaviour
    {
        [SerializeField] private TcpGameClient client;
        [SerializeField] private float moveSpeed = 3f;

        private float _dx;
        private float _dy;

        private void Awake()
        {
            if (client == null) client = FindObjectOfType<TcpGameClient>();
        }

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            float dt = Time.deltaTime > 0f ? Time.deltaTime : 0.02f;
            _dx += h * moveSpeed * dt;
            _dy += v * moveSpeed * dt;
        }

        private void FixedUpdate()
        {
            if (client == null || !client.IsConnected) return;
            if (!PlayerInputSync.AllClientsConnected()) return;

            long frame = GameMain.Instance != null ? GameMain.Instance.CurrentFrame + 1 : 0;
            var msg = new PlayerInputMessage(frame, FixedPoint.FromFloat(_dx), FixedPoint.FromFloat(_dy));
            client.Send(MessageType.PlayerInput, JsonUtility.ToJson(msg));
            _dx = 0f;
            _dy = 0f;
        }
    }
}
