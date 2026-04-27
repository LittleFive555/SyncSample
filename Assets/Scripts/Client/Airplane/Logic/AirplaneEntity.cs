using SyncSample.Client.Gameplay.World.Logic;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Airplane.Logic
{
    public class AirplaneEntity : ILogicUpdate, ICharacterEntity
    {
        
        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool IsLocal { get; private set; }

        private int _input;

        private FixedPoint _logicX;
        public float X => _logicX.ToFloat();
        private FixedPoint _logicY;
        public float Y => _logicY.ToFloat();

        private FixedPoint _moveRangeXMin = FixedPoint.FromFloat(-14);
        private FixedPoint _moveRangeXMax = FixedPoint.FromFloat(14);
        private FixedPoint _moveRangeYMin = FixedPoint.FromFloat(-26);
        private FixedPoint _moveRangeYMax = FixedPoint.FromFloat(26);

        public AirplaneEntity(string id, string name, bool isLocal)
        {
            Id = id;
            Name = name;
            IsLocal = isLocal;
        }

#region ILogicUpdate
        public int Priority => 0;

        public void OnLogicFrame(long frame)
        {
            ApplyMovement(_input.GetHorizontal(), _input.GetVertical());
            if (_input.GetInput(InputType.A))
                Fire();
        }
#endregion

        public void ReceiveInput(long frame, int input)
        {
            _input = input;
        }

        public void SetPosition(float x, float y)
        {
            _logicX = FixedPoint.FromFloat(x);
            _logicY = FixedPoint.FromFloat(y);
        }

        /// <summary> 应用位移：先以定点数加到逻辑，再同步到显示（显示用浮点）。 </summary>
        private void ApplyMovement(float dx, float dy)
        {
            _logicX = FixedPoint.FromFloat(Mathf.Clamp(_logicX.ToFloat() + dx * Const.AirplaneMoveSpeed * GlobalSwitch.Instance.LockstepSwitch.LogicDeltaTime, _moveRangeXMin.ToFloat(), _moveRangeXMax.ToFloat()));
            _logicY = FixedPoint.FromFloat(Mathf.Clamp(_logicY.ToFloat() + dy * Const.AirplaneMoveSpeed * GlobalSwitch.Instance.LockstepSwitch.LogicDeltaTime, _moveRangeYMin.ToFloat(), _moveRangeYMax.ToFloat()));
        }

        private void Fire()
        {
            AirplaneManager.Instance.NewBullet(_logicX, _logicY);
        }
    }
}