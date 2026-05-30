using System;
using EngineRoom.Runtime.Singleton;
using UnityEngine;

namespace EngineRoom.Demo.Singletons
{
    [Singleton]
    public partial class GameManager : MonoBehaviour
    {
        public event Action<int> CountChanged;

        public int Count => _count;

        [Dependency] private ISoundManager _soundManager;
        [Dependency] private IUiManager _uiManager;
        [Dependency] private IDataStoreManager _dataStoreManager;

        private int _count;

        partial void OnStart()
        {
            _count = _dataStoreManager.GetScore();
            _uiManager.SetCount(_count);
        }

        public void RegisterTap()
        {
            _count++;
            _dataStoreManager.SetScore(_count);
            _soundManager.PlayTap();
            _uiManager.SetCount(_count);
            CountChanged?.Invoke(_count);
        }
    }
}
