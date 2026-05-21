using EngineRoom;
using UnityEngine;

namespace TestAssembly
{
    [Singleton]
    public partial class GameManager : MonoBehaviour
    {
        public string GameID { get; set; }
        
        [IgnoreSingletonMember] 
        public int IgnoredInt { get; set; }
        
        partial void OnAwake()
        {
            ISoundManager.Instance.PlaySound();
            ISingleton<ISoundManager>.Instance.PlaySound();
        }
    }
}