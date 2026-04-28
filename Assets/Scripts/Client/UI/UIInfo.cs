using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SyncSample.Client.UI
{
    public interface IInfoSource
    {
        public string Name { get; }
        public Vector2 LogicPosition { get; }
        public Vector2 ViewPosition { get; }
    }

    public class UIInfo : MonoBehaviour
    {
        public static UIInfo Instance;
        [SerializeField] private TextMeshProUGUI _delay;
        [SerializeField] private Transform _posRoot;
        [SerializeField] private TextMeshProUGUI _posTemplate;
        [SerializeField] private TextMeshProUGUI _lockstepStatus;

        private Dictionary<IInfoSource, TextMeshProUGUI> _posMap = new Dictionary<IInfoSource, TextMeshProUGUI>();

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            foreach (var pos in _posMap)
            {
                pos.Value.text = $"{pos.Key.Name} Logic: ({pos.Key.LogicPosition.x}, {pos.Key.LogicPosition.y}), View: ({pos.Key.ViewPosition.x}, {pos.Key.ViewPosition.y})";
            }
        }

        public void SetDelay(long delay)
        {
            _delay.text = $"Delay: {delay}ms";
        }

        public void SetFrame(long frame)
        {
            Logger.Log($"SetFrame: {frame}");
            _lockstepStatus.text = $"Frame: {frame}";
        }

        public void SetFrame(long localFrame, long serverFrame)
        {
            _lockstepStatus.text = $"Local Frame: {localFrame}, Server Frame: {serverFrame}";
        }

        public void RegisterPos(IInfoSource logicSource)
        {
            if (!_posMap.TryGetValue(logicSource, out var pos))
            {
                pos = Instantiate(_posTemplate, _posRoot);
                pos.gameObject.SetActive(true);
                _posMap[logicSource] = pos;
            }
        }

        public void UnregisterPos(IInfoSource logicSource)
        {
            if (_posMap.TryGetValue(logicSource, out var pos))
            {
                Destroy(pos.gameObject);
                _posMap.Remove(logicSource);
            }
        }
    }
}