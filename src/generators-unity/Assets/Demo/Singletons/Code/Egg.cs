using System;
using EngineRoom.Runtime.Singleton;
using UnityEngine;
using UnityEngine.UI;

namespace EngineRoom.Demo.Singletons
{
    public partial class Egg : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _image;
        [SerializeField] private float _transitionDuration = 0.3f;
        [SerializeField] private EggState[] _states;

        [Dependency] private IGameManager _gameManager;
        [Dependency] private ISoundManager _soundManager;

        private int _currentStateIndex;

        private void Awake()
        {
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
            _ = PlayStateTransition();
        }

        private int ResolveStateIndex(int taps) 
            => Array.FindLastIndex(_states, s => taps >= s.TapsRequired);

        private void ApplyStateSprite(int index) 
            => _image.sprite = _states[Mathf.Clamp(index, 0, _states.Length - 1)].Sprite;

        private async Awaitable PlayStateTransition()
        {
            _button.interactable = false;
            
            ApplyStateSprite(_currentStateIndex);
            _soundManager.PlayStateChange();
            await Awaitable.WaitForSecondsAsync(_transitionDuration);
            
            _button.interactable = true;
        }
    }
}