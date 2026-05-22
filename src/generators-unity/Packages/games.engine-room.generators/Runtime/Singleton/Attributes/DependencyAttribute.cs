using System;

namespace EngineRoom.Runtime.Singleton
{
    /// <summary>
    /// Marks a private or protected instance field on a MonoBehaviour-derived
    /// class as a dependency on a singleton. The field type must be an interface
    /// extending <see cref="ISingleton{TInterface}"/> with itself as the type
    /// argument. The source generator emits a partial <c>Start()</c> that
    /// assigns the field from its interface's static <c>Instance</c> slot before
    /// calling the user-provided <c>OnStart()</c> hook.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DependencyAttribute : Attribute
    {
    }
}
