using SyncSample.Client.Gameplay.World.Logic;
using SyncSample.Common;

namespace SyncSample.Client.Airplane.Logic
{
    public class EnemyEntity : ILogicUpdate
    {
        private static long _idCounter = 0;
        private readonly long _id;
        public long Id => _id;
        private FixedPoint _logicX;
        public float X => _logicX.ToFloat();
        private FixedPoint _logicY;
        public float Y => _logicY.ToFloat();


        private FixedPoint _sizeX = FixedPoint.FromFloat(4);
        private FixedPoint _sizeY = FixedPoint.FromFloat(4);

        public EnemyEntity(FixedPoint x, FixedPoint y)
        {
            _id = _idCounter++;
            _logicX = x;
            _logicY = y;
        }

        public bool IsHit(FixedPoint x, FixedPoint y)
        {
            return x.ToFloat() > X - _sizeX.ToFloat() / 2 
                && x.ToFloat() < X + _sizeX.ToFloat() / 2 
                && y.ToFloat() > Y - _sizeY.ToFloat() / 2 
                && y.ToFloat() < Y + _sizeY.ToFloat() / 2;
        }

        public int Priority => 1;
        public void OnLogicFrame(long frame)
        {
            // 向下移动
            _logicY = FixedPoint.FromFloat(_logicY.ToFloat() - Const.EnemyMoveSpeed * GlobalSwitch.Instance.LockstepSwitch.LogicDeltaTime);
        }
    }
}