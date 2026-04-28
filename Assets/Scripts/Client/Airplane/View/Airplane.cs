using SyncSample.Client.Airplane.Logic;
using SyncSample.Client.UI;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Airplane.View
{
    public class Airplane : MonoBehaviour, IInfoSource
    {
        public AirplaneEntity Entity { get; private set; }

        public string Name => Entity.Name;

        public Vector2 LogicPosition => new Vector2(Entity.X, Entity.Y);

        public Vector2 ViewPosition => transform.localPosition;

        private bool _isCatchingUp = false;

        public void Init(AirplaneEntity characterEntity)
        {
            Entity = characterEntity;
            UIInfo.Instance.RegisterPos(this);
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
                    Vector3 onlyX = Vector3.MoveTowards(transform.localPosition, new Vector3(Entity.X, transform.localPosition.y, 0), Const.AirplaneMoveSpeed * Time.deltaTime);
                    Vector3 onlyY = Vector3.MoveTowards(transform.localPosition, new Vector3(transform.localPosition.x, Entity.Y, 0), Const.AirplaneMoveSpeed * Time.deltaTime);
                    transform.localPosition = new Vector3(onlyX.x, onlyY.y, 0);
                }
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