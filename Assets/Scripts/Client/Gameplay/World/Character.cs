using SyncSample.Client.UI;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 角色：逻辑与显示分离。逻辑位置为定点数，仅在同步到显示时用 FixedPoint 转为浮点。
    /// </summary>
    public class Character : MonoBehaviour
    {
        [SerializeField] private Transform displayRoot;
        [SerializeField] private float moveSpeed = 3f;

        public string Id { get; private set; }
        public string Name { get; private set; }

        private FixedPoint _logicX;
        private FixedPoint _logicY;

        /// <summary> 逻辑位置 X（定点数，仅读时转浮点）。 </summary>
        public float LogicX { get { return _logicX.ToFloat(); } }
        /// <summary> 逻辑位置 Y。 </summary>
        public float LogicY { get { return _logicY.ToFloat(); } }

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
            UIInfo.Instance.SetPos(this);
        }

        /// <summary> 应用位移：先以定点数加到逻辑，再同步到显示（显示用浮点）。 </summary>
        public void ApplyMovement(float dx, float dy)
        {
            _logicX = FixedPoint.FromFloat(_logicX.ToFloat() + dx * moveSpeed * Time.fixedDeltaTime);
            _logicY = FixedPoint.FromFloat(_logicY.ToFloat() + dy * moveSpeed * Time.fixedDeltaTime);
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
    }
}
