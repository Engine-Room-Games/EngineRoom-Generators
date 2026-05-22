#nullable enable
using System;

namespace EngineRoom.Runtime.Singleton
{
    /// <summary>
    /// Marks a MonoBehaviour as a singleton. The source generator emits a matching
    /// I&lt;ClassName&gt; interface and a partial Awake implementation that publishes
    /// the instance and optionally keeps the host GameObject across scene loads.
    /// Members exposed on the interface are either those tagged with
    /// <see cref="SingletonIncludeAttribute"/> (explicit mode) or every public
    /// instance method and property declared on the class minus those tagged with
    /// <see cref="SingletonIgnoreAttribute"/> (auto mode, used when no member
    /// is tagged with <see cref="SingletonIncludeAttribute"/>). Supplying
    /// <paramref name="interfaceType"/> suppresses interface generation entirely —
    /// the user-supplied interface must extend <see cref="ISingleton{TInterface}"/>
    /// and be present in the class's base list.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SingletonAttribute : Attribute
    {
        public bool DestroyOnLoad { get; }
        public Type? Interface { get; }

        public SingletonAttribute(bool destroyOnLoad = false)
        {
            DestroyOnLoad = destroyOnLoad;
        }

        public SingletonAttribute(Type interfaceType, bool destroyOnLoad = false)
        {
            Interface = interfaceType;
            DestroyOnLoad = destroyOnLoad;
        }
    }
}
