using SyncSample.Client.Airplane.Logic;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Airplane.View
{
    public class Enemy : MonoBehaviour
    {
        public EnemyEntity Entity { get; private set; }

        private bool _isCatchingUp = false;

        public void Init(EnemyEntity enemyEntity)
        {
            Entity = enemyEntity;
            transform.localPosition = new Vector3(Entity.X, Entity.Y, 0);
        }

        private void Update()
        {
            if (GlobalSwitch.Instance.ClientInterpolation)
            {
                if (AirplaneWorldManager.Instance.IsCatchingUp)
                {
                    _isCatchingUp = true;
                    transform.localPosition = new Vector3(Entity.X, Entity.Y, 0);
                    return;
                }
                else
                {
                    if (_isCatchingUp)
                    {
                        _isCatchingUp = false;
                        transform.localPosition = new Vector3(Entity.X, Entity.Y, 0);
                        return;
                    }
                    // 为了简单处理，逻辑和显示上，都将X和Y分开处理
                    Vector3 onlyX = Vector3.MoveTowards(transform.localPosition, new Vector3(Entity.X, transform.localPosition.y, 0), Const.EnemyMoveSpeed * Time.deltaTime);
                    Vector3 onlyY = Vector3.MoveTowards(transform.localPosition, new Vector3(transform.localPosition.x, Entity.Y, 0), Const.EnemyMoveSpeed * Time.deltaTime);
                    transform.localPosition = new Vector3(onlyX.x, onlyY.y, 0);
                }
            }
            else
            {
                transform.localPosition = new Vector3(Entity.X, Entity.Y, 0);
            }
        }
    }
}