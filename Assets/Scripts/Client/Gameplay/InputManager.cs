using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 采集上下左右输入；在 Update 中检测逻辑帧推进，每逻辑帧仅发送一次（含空操作）。
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        private float _dx;
        private float _dy;

        public static void Initialize()
        {
            var obj = new GameObject("InputManager");
            Instance = obj.AddComponent<InputManager>();
            DontDestroyOnLoad(obj);
        }

        private void Update()
        {
            _dx = Input.GetAxisRaw("Horizontal");
            _dy = Input.GetAxisRaw("Vertical");
        }

        public void GetInput(out float dx, out float dy)
        {
            dx = _dx;
            dy = _dy;
        }
    }
}
