using System;

namespace EngineRoom.Runtime.Singleton
{
    /// <summary>
    /// Marks a method, property, or event on a <see cref="SingletonAttribute"/>-decorated class
    /// to be projected onto the generated singleton interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
    public sealed class SingletonIncludeAttribute : Attribute
    {
    }
}
