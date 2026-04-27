using System.Collections.Generic;
using SyncSample.Client.Gameplay.World.Logic;
using SyncSample.Common;

namespace SyncSample.Client.Airplane.Logic
{
    public class BulletEntity : ILogicUpdate
    {
        private static long _idCounter = 0;

        private readonly long _id;
        public long Id => _id;
        private FixedPoint _logicX;
        public float X => _logicX.ToFloat();
        private FixedPoint _logicY;
        public float Y => _logicY.ToFloat();

        public BulletEntity(FixedPoint x, FixedPoint y)
        {
            _id = _idCounter++;
            _logicX = x;
            _logicY = y;
        }


        public int Priority => 3;
        public void OnLogicFrame(long frame)
        {
            // 只向上移动
            _logicY = FixedPoint.FromFloat(_logicY.ToFloat() + Const.BulletMoveSpeed * GlobalSwitch.Instance.LockstepSwitch.LogicDeltaTime);
            
            List<EnemyEntity> toRemoveEnemies = new List<EnemyEntity>();
            foreach (var enemy in AirplaneManager.Instance.EnemyEntities.Values)
            {
                if (enemy.IsHit(_logicX, _logicY))
                {
                    toRemoveEnemies.Add(enemy);
                }
            }
            foreach (var enemy in toRemoveEnemies)
            {
                AirplaneManager.Instance.RemoveBullet(this);
                AirplaneManager.Instance.RemoveEnemy(enemy);
            }
        }
    }
}