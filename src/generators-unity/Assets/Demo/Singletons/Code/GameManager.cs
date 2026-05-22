using EngineRoom.Runtime.Singleton;
using UnityEngine;

namespace EngineRoom.Demo.Singletons
{
    [Singleton]
    public partial class GameManager : MonoBehaviour
    {
        public int Count => _count;

        [Dependency] private ISoundManager _soundManager;
        [Dependency] private IUiManager _uiManager;
        [Dependency] private IDataStoreManager _dataStoreManager;

        private int _count;

        public void RegisterTap()
        {
            _count++;
            _dataStoreManager.SetScore(_count);
            _soundManager.PlayTap();
            _uiManager.SetCount(_count);
        }

        partial void OnStart()
        {
            _count = _dataStoreManager.GetScore();
            _uiManager.SetCount(_count);
        }
    }
}
