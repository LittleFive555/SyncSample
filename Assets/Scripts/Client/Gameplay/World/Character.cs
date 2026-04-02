using SyncSample.Client.UI;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 角色：逻辑与显示分离。逻辑位置为定点数，仅在同步到显示时用 FixedPoint 转为浮点。
    /// </summary>
    public class Character : MonoBehaviour, ILogicEntity
    {
        [SerializeField] private Transform displayRoot;
        [SerializeField] private float moveSpeed = 3f;

        public string Id { get; private set; }
        public string Name { get; private set; }

        private FixedPoint _dx;
        private FixedPoint _dy;

        private FixedPoint _logicX;
        private FixedPoint _logicY;

        /// <summary> 逻辑位置 X（定点数，仅读时转浮点）。 </summary>
        public float LogicX { get { return _logicX.ToFloat(); } }
        /// <summary> 逻辑位置 Y。 </summary>
        public float LogicY { get { return _logicY.ToFloat(); } }

        public int Priority => 0;


        private void Awake()
        {
            if (displayRoot == null)
                displayRoot = transform;
            SyncLogicFromDisplay();
        }

        public void Init(string id, string name, float x, float y)
        {
            Id = id;
            Name = name;
            SetLogicPosition(x, y);
            UIInfo.Instance.RegisterPos(this);
            if (GlobalSwitch.Instance.UseLockstep)
                LockstepWorldManager.Instance.RegisterLogicEntity(this);
        }

        private void Update()
        {
            if (GlobalSwitch.Instance.LockstepSwitch.Interpolation)
            {
                // 为了简单处理，逻辑和显示上，都将X和Y分开处理
                Vector3 onlyX = Vector3.MoveTowards(displayRoot.localPosition, new Vector3(LogicX, displayRoot.localPosition.y, 0), moveSpeed * Time.deltaTime);
                Vector3 onlyY = Vector3.MoveTowards(displayRoot.localPosition, new Vector3(displayRoot.localPosition.x, LogicY, 0), moveSpeed * Time.deltaTime);
                displayRoot.localPosition = new Vector3(onlyX.x, onlyY.y, 0);
            }
        }

        private void OnDestroy()
        {
            UIInfo.Instance.UnregisterPos(this);
            if (GlobalSwitch.Instance.UseLockstep)
                LockstepWorldManager.Instance.UnregisterLogicEntity(this);
        }

        public void ReceiveInput(long frame, FixedPoint dx, FixedPoint dy)
        {
            _dx = dx;
            _dy = dy;
        }

        /// <summary> 应用位移：先以定点数加到逻辑，再同步到显示（显示用浮点）。 </summary>
        public void ApplyMovement(float dx, float dy)
        {
            _logicX = FixedPoint.FromFloat(_logicX.ToFloat() + dx * moveSpeed * GlobalSwitch.Instance.LockstepSwitch.LogicDeltaTime);
            _logicY = FixedPoint.FromFloat(_logicY.ToFloat() + dy * moveSpeed * GlobalSwitch.Instance.LockstepSwitch.LogicDeltaTime);
            if (!GlobalSwitch.Instance.LockstepSwitch.Interpolation)
                SyncDisplayFromLogic();
        }

        /// <summary> 设置逻辑位置（浮点入，内部存定点）后同步到显示。 </summary>
        public void SetLogicPosition(float x, float y)
        {
            _logicX = FixedPoint.FromFloat(x);
            _logicY = FixedPoint.FromFloat(y);
            SyncDisplayFromLogic();
        }

        /// <summary> 从当前显示读取到逻辑（初始化或校正用），转为定点存储。 </summary>
        public void SyncLogicFromDisplay()
        {
            Vector3 p = displayRoot.localPosition;
            _logicX = FixedPoint.FromFloat(p.x);
            _logicY = FixedPoint.FromFloat(p.y);
        }

        /// <summary> 将逻辑位置（定点）用 FixedPoint 转为浮点后应用到显示。 </summary>
        public void SyncDisplayFromLogic()
        {
            Vector3 p = displayRoot.localPosition;
            p.x = _logicX.ToFloat();
            p.y = _logicY.ToFloat();
            displayRoot.localPosition = p;
        }

        public void OnLogicFrame(long frame)
        {
            ApplyMovement(_dx.ToFloat(), _dy.ToFloat());
        }

        /// <summary> 应用服务器下发的世界状态（位置与当前输入方向）。 </summary>
        public void ApplyWorldState(float x, float y, float dx, float dy)
        {
            SetLogicPosition(x, y);
            _dx = FixedPoint.FromFloat(dx);
            _dy = FixedPoint.FromFloat(dy);
        }

    }
}
