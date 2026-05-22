using EngineRoom.Runtime.Singleton;
using UnityEngine;

namespace TestAssembly
{
    [Singleton(typeof(ISoundManager))]
    public partial class SoundManager : MonoBehaviour, ISoundManager
    {
        public void PlaySound() => Debug.Log("Sound Played");
    }

    public partial interface ISoundManager
    {
        void PlaySound();
    }
}