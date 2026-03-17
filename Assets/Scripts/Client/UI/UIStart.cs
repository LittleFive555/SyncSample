using UnityEngine;
using UnityEngine.UI;

namespace SyncSample.Client.UI
{
    public class UIStart : MonoBehaviour
    {
        [SerializeField]
        private Button btnStart;

        [SerializeField]
        private TcpGameClient tcpGameClient;

        private void Awake()
        {
            btnStart.onClick.AddListener(ConnectServer);
        }

        private void ConnectServer()
        {
            tcpGameClient.Connect();
            gameObject.SetActive(false);
        }
    }
}