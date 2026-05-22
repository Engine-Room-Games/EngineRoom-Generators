using EngineRoom.Runtime.Singleton;
using UnityEngine;

namespace EngineRoom.Demo.Singletons
{
    [Singleton]
    public partial class GameManager : MonoBehaviour
    {
        private const string CountKey = "EggCount";
        
        public int Count => _count;

        [Dependency] private ISoundManager _soundManager;
        [Dependency] private IUIManager _uiManager;
        
        private int _count;

        public void RegisterTap()
        {
            _count++;
            Save();
            _soundManager.PlayTap();
            _uiManager.SetCount(_count);
        }

        partial void OnAwake()
        {
            _count = PlayerPrefs.GetInt(CountKey, 0);
        }

        partial void OnStart()
        {
            _uiManager.SetCount(_count);
        }

        private void Save()
        {
            PlayerPrefs.SetInt(CountKey, _count);
            PlayerPrefs.Save();
        }
    }
}
