using SyncSample.Client.Airplane.Logic;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Airplane.View
{
    public class Bullet : MonoBehaviour
    {
        public BulletEntity Entity { get; private set; }

        public void Init(BulletEntity bulletEntity)
        {
            Entity = bulletEntity;
            transform.localPosition = new Vector3(Entity.X, Entity.Y, 0);
        }

        private void Update()
        {
            if (GlobalSwitch.Instance.ClientInterpolation)
            {
                // 为了简单处理，逻辑和显示上，都将X和Y分开处理
                Vector3 onlyX = Vector3.MoveTowards(transform.localPosition, new Vector3(Entity.X, transform.localPosition.y, 0), Const.BulletMoveSpeed * Time.deltaTime);
                Vector3 onlyY = Vector3.MoveTowards(transform.localPosition, new Vector3(transform.localPosition.x, Entity.Y, 0), Const.BulletMoveSpeed * Time.deltaTime);
                transform.localPosition = new Vector3(onlyX.x, onlyY.y, 0);
            }
            else
            {
                transform.localPosition = new Vector3(Entity.X, Entity.Y, 0);
            }
        }
    }
}