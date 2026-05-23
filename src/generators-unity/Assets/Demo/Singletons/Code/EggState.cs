using System;
using UnityEngine;

namespace EngineRoom.Demo.Singletons
{
    [Serializable]
    public class EggState
    {
        [field: SerializeField] public int TapsRequired { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
    }
}