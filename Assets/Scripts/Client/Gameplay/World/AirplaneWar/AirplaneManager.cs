using System.Collections.Generic;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    public class AirplaneManager : MonoBehaviour
    {
        public static AirplaneManager Instance { get; private set; }

        [SerializeField] private Airplane _p1;
        [SerializeField] private Airplane _p2;

        private Dictionary<string, Airplane> _airplanes = new Dictionary<string, Airplane>();
        
        public string SelfId;

        private void Awake()
        {
            Instance = this;
        }

        public void EnsurePlayer(string id, PlayerType playerType)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (_airplanes.ContainsKey(id))
                return;

            Airplane airplane = playerType == PlayerType.P1 ? _p1 : _p2;
            airplane.Init(id, id, 0f, 0f);
            _airplanes[id] = airplane;
        }

        /// <summary> 应用服务器下发的位移，在 WaitForAllClientsThisFrame 中被调用：先应用到逻辑，再同步到显示。 </summary>
        public void ReceiveInput(string clientId, long frame, FixedPoint dx, FixedPoint dy)
        {
            if (string.IsNullOrEmpty(clientId) || !_airplanes.TryGetValue(clientId, out Airplane airplane) || airplane == null)
                return;
            airplane.ReceiveInput(frame, dx, dy);
        }
    }
}