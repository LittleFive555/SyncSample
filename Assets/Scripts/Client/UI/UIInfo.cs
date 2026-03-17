using System.Collections.Generic;
using SyncSample.Client.Gameplay;
using TMPro;
using UnityEngine;

namespace SyncSample.Client.UI
{
    public class UIInfo : MonoBehaviour
    {
        public static UIInfo Instance;
        [SerializeField] private TextMeshProUGUI _delay;
        [SerializeField] private Transform _posRoot;
        [SerializeField] private TextMeshProUGUI _posTemplate;

        private Dictionary<Character, TextMeshProUGUI> _posMap = new Dictionary<Character, TextMeshProUGUI>();

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            foreach (var pos in _posMap)
            {
                pos.Value.text = $"{pos.Key.Name} Logic: ({pos.Key.LogicX}, {pos.Key.LogicY}), View: ({pos.Key.transform.localPosition.x}, {pos.Key.transform.localPosition.y})";
            }
        }

        public void SetDelay(long delay)
        {
            _delay.text = $"Delay: {delay}ms";
        }

        public void RegisterPos(Character character)
        {
            if (!_posMap.TryGetValue(character, out var pos))
            {
                pos = Instantiate(_posTemplate, _posRoot);
                pos.gameObject.SetActive(true);
                _posMap[character] = pos;
            }
        }

        public void UnregisterPos(Character character)
        {
            if (_posMap.TryGetValue(character, out var pos))
            {
                Destroy(pos.gameObject);
                _posMap.Remove(character);
            }
        }
    }
}