using EngineRoom.Runtime.Singleton;
using UnityEngine;

namespace EngineRoom.Demo.Singletons
{
    [Singleton(typeof(IDataStoreManager))]
    public partial class DataStoreManager : MonoBehaviour, IDataStoreManager
    {
        private const string ScoreKey = "EggCount";

        public int GetScore()
        {
            return PlayerPrefs.GetInt(ScoreKey, 0);
        }

        public void SetScore(int value)
        {
            PlayerPrefs.SetInt(ScoreKey, value);
            PlayerPrefs.Save();
        }
    }
}
