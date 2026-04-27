using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 采集上下左右输入；在 Update 中检测逻辑帧推进，每逻辑帧仅发送一次（含空操作）。
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        private int _input = 0;

        public static void Initialize()
        {
            var obj = new GameObject("InputManager");
            Instance = obj.AddComponent<InputManager>();
            DontDestroyOnLoad(obj);
        }

        private void Update()
        {
            var dx = Input.GetAxisRaw("Horizontal");
            if (!Mathf.Approximately(dx, 0))
            {
                if (dx > 0) 
                    _input = _input.SetInput(InputType.Right, true);
                else
                    _input = _input.SetInput(InputType.Left, true);
            }
            var dy = Input.GetAxisRaw("Vertical");
            if (!Mathf.Approximately(dy, 0))
            {
                if (dy > 0) 
                    _input = _input.SetInput(InputType.Up, true);
                else
                    _input = _input.SetInput(InputType.Down, true);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _input = _input.SetInput(InputType.A, true);
            }
        }

        public int GetInput()
        {
            int input = _input;
            _input = 0;
            return input;
        }
    }
}
