using EngineRoom.Runtime.Singleton;
using UnityEngine;

namespace TestAssembly
{
    [Singleton]
    public partial class GameManager : MonoBehaviour
    {
        public string GameID { get; set; }
        
        [SingletonIgnore]
        public int IgnoredInt { get; set; }
        
        partial void OnAwake()
        {
            ISoundManager.Instance.PlaySound(); 
            ISingleton<ISoundManager>.Instance.PlaySound();
        }
    }
}