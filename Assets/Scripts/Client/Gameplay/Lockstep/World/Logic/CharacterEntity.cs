using SyncSample.Client.Gameplay.World.Logic;
using SyncSample.Common;

namespace SyncSample.Client.Gameplay.Lockstep.World.Logic
{
    public class CharacterEntity : ILogicEntity, ICharacterEntity
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool IsLocal { get; private set; }

        private FixedPoint _dx;
        public float DeltaX => _dx.ToFloat();
        private FixedPoint _dy;
        public float DeltaY => _dy.ToFloat();

        private FixedPoint _logicX;
        public float X => _logicX.ToFloat();
        private FixedPoint _logicY;
        public float Y => _logicY.ToFloat();

        public const float MoveSpeed = 3f;

        public CharacterEntity(string id, string name, bool isLocal)
        {
            Id = id;
            Name = name;
            IsLocal = isLocal;
        }

#region ILogicEntity
        public int Priority => 0;

        public void OnLogicFrame(long frame)
        {
            ApplyMovement(_dx.ToFloat(), _dy.ToFloat());
        }
#endregion

        public void ReceiveInput(long frame, FixedPoint dx, FixedPoint dy)
        {
            _dx = dx;
            _dy = dy;
        }

        public void SetPosition(float x, float y)
        {
            _logicX = FixedPoint.FromFloat(x);
            _logicY = FixedPoint.FromFloat(y);
        }

        /// <summary> 应用位移：先以定点数加到逻辑，再同步到显示（显示用浮点）。 </summary>
        private void ApplyMovement(float dx, float dy)
        {
            _logicX = FixedPoint.FromFloat(_logicX.ToFloat() + dx * MoveSpeed * GlobalSwitch.Instance.LockstepSwitch.LogicDeltaTime);
            _logicY = FixedPoint.FromFloat(_logicY.ToFloat() + dy * MoveSpeed * GlobalSwitch.Instance.LockstepSwitch.LogicDeltaTime);
        }
    }
}
