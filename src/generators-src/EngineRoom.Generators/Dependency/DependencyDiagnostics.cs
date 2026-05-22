using Microsoft.CodeAnalysis;

namespace EngineRoom.Generators.Dependency
{
    internal static class DependencyDiagnostics
    {
        private const string Category = "EngineRoom.Runtime.Singleton";

        public static readonly DiagnosticDescriptor MustBeMonoBehaviour = new DiagnosticDescriptor(
            id: "ERG0101",
            title: "[Dependency] must live on a MonoBehaviour",
            messageFormat: "Class '{0}' has a [Dependency] field but does not inherit from UnityEngine.MonoBehaviour",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MustBePartial = new DiagnosticDescriptor(
            id: "ERG0102",
            title: "[Dependency] host class must be partial",
            messageFormat: "Class '{0}' has a [Dependency] field and must be declared partial",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MustNotDefineStart = new DiagnosticDescriptor(
            id: "ERG0103",
            title: "[Dependency] host class must not define its own Start",
            messageFormat: "Class '{0}' has a [Dependency] field and must not define a Start() method. Move that code into OnStart() instead.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor FieldMustBeNonPublicInstance = new DiagnosticDescriptor(
            id: "ERG0104",
            title: "[Dependency] field must be a private or protected instance field",
            messageFormat: "Field '{0}' is marked [Dependency] and must be a private or protected instance field",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor FieldTypeMustBeSingletonInterface = new DiagnosticDescriptor(
            id: "ERG0105",
            title: "[Dependency] field type must be an ISingleton<TSelf> interface",
            messageFormat: "Field '{0}' is marked [Dependency] but its type '{1}' is not an interface extending ISingleton<{1}>",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
