using System;

namespace EngineRoom.Runtime.Singleton
{
    /// <summary>
    /// Marks a method or property on a <see cref="SingletonAttribute"/>-decorated class
    /// to be projected onto the generated singleton interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class SingletonIncludeAttribute : Attribute
    {
    }
}
