using UnityEngine;
using UnityEngine.UI;

namespace SyncSample.Client.UI
{
    public class UIStart : MonoBehaviour
    {
        [SerializeField]
        private Button btnStart;

        private void Awake()
        {
            btnStart.onClick.AddListener(ConnectServer);
        }

        private void ConnectServer()
        {
            GameMain.Instance.ConnectServer();
            gameObject.SetActive(false);
        }
    }
}