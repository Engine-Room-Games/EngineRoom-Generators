namespace EngineRoom.Generators.Dependency
{
    internal static class DependencyConstants
    {
        public static readonly string AttributeFullName = typeof(global::EngineRoom.Runtime.Singleton.DependencyAttribute).FullName!;

        // ISingleton<T> can't be linked into this netstandard2.0 generator (its
        // static interface member predates the language level we target), so we
        // identify the open generic by namespace + name + arity instead of
        // typeof().
        public const string SingletonInterfaceNamespace = "EngineRoom.Runtime.Singleton";
        public const string SingletonInterfaceName = "ISingleton";
        public const int SingletonInterfaceArity = 1;

        public const string MonoBehaviourFullyQualifiedName = "global::UnityEngine.MonoBehaviour";
    }
}
