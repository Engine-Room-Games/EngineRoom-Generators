using System;
using System.Collections;
using EngineRoom.Runtime.Singleton;
using UnityEngine;
using UnityEngine.UI;

namespace EngineRoom.Demo.Singletons
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public partial class Egg : MonoBehaviour
    {
        [SerializeField] private State[] _states;
        [SerializeField] private float _shakeDuration = 0.3f;
        [SerializeField] private float _shakeIntensity = 8f;

        [Dependency] private IGameManager _gameManager;
        [Dependency] private ISoundManager _soundManager;
        
        private Button _button;
        private Image _image;
        private RectTransform _rectTransform;
        private Vector2 _basePosition;
        private int _currentStateIndex;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
            _rectTransform = (RectTransform)transform;
            _basePosition = _rectTransform.anchoredPosition;
            _button.onClick.AddListener(OnTapped);
        }

        partial void OnStart()
        {
            _currentStateIndex = ResolveStateIndex(_gameManager.Count);
            ApplyStateSprite(_currentStateIndex);
        }

        private void OnTapped()
        {
            _gameManager.RegisterTap();

            var nextStateIndex = ResolveStateIndex(_gameManager.Count);
            
            if (nextStateIndex == _currentStateIndex)
            {
                return;
            }
            
            _currentStateIndex = nextStateIndex;
            StartCoroutine(PlayStateTransition());
        }

        private int ResolveStateIndex(int taps)
        {
            var result = -1;
            for (var i = 0; i < _states.Length; i++)
            {
                if (taps >= _states[i].TapsRequired)
                {
                    result = i;
                }
            }
            return result;
        }

        private void ApplyStateSprite(int index)
        {
            if (index >= 0 && index < _states.Length && _states[index].Sprite)
            {
                _image.sprite = _states[index].Sprite;
            }
        }

        private IEnumerator PlayStateTransition()
        {
            _button.interactable = false;
            ApplyStateSprite(_currentStateIndex);
            _soundManager.PlayStateChange();

            var elapsed = 0f;
            while (elapsed < _shakeDuration)
            {
                elapsed += Time.deltaTime;
                var falloff = 1f - (elapsed / _shakeDuration);
                var offset = Mathf.Sin(elapsed * 50f) * _shakeIntensity * falloff;
                _rectTransform.anchoredPosition = _basePosition + new Vector2(offset, 0f);
                yield return null;
            }

            _rectTransform.anchoredPosition = _basePosition;
            _button.interactable = true;
        }

        [Serializable]
        public class State
        {
            [field: SerializeField] public int TapsRequired { get; private set; }
            [field: SerializeField] public Sprite Sprite { get; private set; }
        }
    }
}
