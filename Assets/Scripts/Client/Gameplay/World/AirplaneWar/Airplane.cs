using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    public enum PlayerType
    {
        P1,
        P2,
    }
    public class Airplane : Character
    {
        [SerializeField] private PlayerType _playerType;
        public PlayerType PlayerType => _playerType;
    }
}