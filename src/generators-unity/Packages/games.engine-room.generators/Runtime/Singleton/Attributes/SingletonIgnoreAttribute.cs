using System;

namespace EngineRoom.Runtime.Singleton
{
    /// <summary>
    /// On a <see cref="SingletonAttribute"/>-decorated class that has no member
    /// tagged with <see cref="SingletonIncludeAttribute"/>, marks a public method or
    /// property to be excluded from the generated singleton interface. Has no
    /// effect when the class is in explicit mode (i.e. uses
    /// <see cref="SingletonIncludeAttribute"/> elsewhere).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class SingletonIgnoreAttribute : Attribute
    {
    }
}
