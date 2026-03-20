using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 采集上下左右输入；在 Update 中检测逻辑帧推进，每逻辑帧仅发送一次（含空操作）。
    /// </summary>
    public class LockstepInputManager : MonoBehaviour
    {
        private float _dx;
        private float _dy;
        private long _lastSentFrame = -1;

        public static void Initialize()
        {
            var obj = new GameObject("LockstepInputManager");
            obj.AddComponent<LockstepInputManager>();
            DontDestroyOnLoad(obj);
        }

        private void Awake()
        {
        }

        private void Update()
        {
            _dx = Input.GetAxisRaw("Horizontal");
            _dy = Input.GetAxisRaw("Vertical");
            SendInput();
        }

        private void SendInput()
        {
            if (!LockstepPlayerInputSync.AllClientsConnected()) return;

            long currentFrame = LockstepWorldManager.Instance.CurrentFrame;
            if (currentFrame <= _lastSentFrame)
                return;

            _lastSentFrame = currentFrame;
            var msg = new PlayerInputMessage(currentFrame, FixedPoint.FromFloat(_dx), FixedPoint.FromFloat(_dy));
            string json = JsonUtility.ToJson(msg);
            int sendDelayMs = GlobalSwitch.Instance != null ? GlobalSwitch.Instance.AddSendDelay : 0;
            if (sendDelayMs > 0)
            {
                var c = GameMain.Instance.Client;
                GameMain.Instance.GameLooper.RunAfterDelayMilliseconds(sendDelayMs, () =>
                {
                    if (c != null && c.IsConnected)
                        c.Send(MessageType.PlayerInput, json);
                });
            }
            else
                GameMain.Instance.Client.Send(MessageType.PlayerInput, json);
        }
    }
}
