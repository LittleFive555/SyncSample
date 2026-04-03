using SyncSample.Client.Gameplay.World.Logic;
using SyncSample.Client.UI;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay.World.View
{
    /// <summary>
    /// 角色：逻辑与显示分离。逻辑位置为定点数，仅在同步到显示时用 FixedPoint 转为浮点。
    /// </summary>
    public class Character : MonoBehaviour, IInfoSource
    {
        public ICharacterEntity Entity { get; private set; }

        public string Name => Entity.Name;

        public Vector2 LogicPosition => new Vector2(Entity.X, Entity.Y);

        public Vector2 ViewPosition => transform.localPosition;

        public void Init(ICharacterEntity characterEntity)
        {
            Entity = characterEntity;
            UIInfo.Instance.RegisterPos(this);
            transform.localPosition = new Vector3(Entity.X, Entity.Y, 0);
        }

        private void Update()
        {
            if (GlobalSwitch.Instance.ClientInterpolation)
            {
                // 为了简单处理，逻辑和显示上，都将X和Y分开处理
                Vector3 onlyX = Vector3.MoveTowards(transform.localPosition, new Vector3(Entity.X, transform.localPosition.y, 0), Const.MoveSpeed * Time.deltaTime);
                Vector3 onlyY = Vector3.MoveTowards(transform.localPosition, new Vector3(transform.localPosition.x, Entity.Y, 0), Const.MoveSpeed * Time.deltaTime);
                transform.localPosition = new Vector3(onlyX.x, onlyY.y, 0);
            }
            else
            {
                transform.localPosition = new Vector3(Entity.X, Entity.Y, 0);
            }
        }

        private void OnDestroy()
        {
            UIInfo.Instance.UnregisterPos(this);
        }
    }
}
