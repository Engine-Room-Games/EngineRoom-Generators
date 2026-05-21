using Microsoft.CodeAnalysis;

namespace EngineRoom.Generators.Singleton
{
    internal static class SingletonDiagnostics
    {
        private const string Category = "EngineRoom.Singleton";

        public static readonly DiagnosticDescriptor MustBeMonoBehaviour = new DiagnosticDescriptor(
            id: "ERG0001",
            title: "[Singleton] must be applied to a MonoBehaviour",
            messageFormat: "Class '{0}' is decorated with [Singleton] but does not inherit from UnityEngine.MonoBehaviour",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MustBePartial = new DiagnosticDescriptor(
            id: "ERG0002",
            title: "[Singleton] class must be partial",
            messageFormat: "Class '{0}' is decorated with [Singleton] and must be declared partial",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MustNotDefineAwake = new DiagnosticDescriptor(
            id: "ERG0003",
            title: "[Singleton] class must not define its own Awake",
            messageFormat: "Class '{0}' is decorated with [Singleton] and must not define an Awake() method. Move that code into OnAwake() instead.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MemberMustBePublic = new DiagnosticDescriptor(
            id: "ERG0004",
            title: "[SingletonMember] must be public",
            messageFormat: "Member '{0}' is marked [SingletonMember] and must be declared public to appear on the generated singleton interface",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MemberMustBeInstance = new DiagnosticDescriptor(
            id: "ERG0005",
            title: "[SingletonMember] must not be static",
            messageFormat: "Member '{0}' is marked [SingletonMember] and must be an instance member, not static",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor IgnoreUnusedInExplicitMode = new DiagnosticDescriptor(
            id: "ERG0006",
            title: "[IgnoreSingletonMember] has no effect when [SingletonMember] is used on the class",
            messageFormat: "Member '{0}' is marked [IgnoreSingletonMember] but the containing class uses [SingletonMember] for explicit selection. Remove [IgnoreSingletonMember] or drop the [SingletonMember] usages.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CustomInterfaceNotImplemented = new DiagnosticDescriptor(
            id: "ERG0007",
            title: "[Singleton] custom interface must be listed in the class base list",
            messageFormat: "Class '{0}' is decorated with [Singleton(typeof({1}))] but does not list '{1}' in its base list. Add it next to the class declaration.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CustomInterfaceMustBeInterface = new DiagnosticDescriptor(
            id: "ERG0008",
            title: "[Singleton] custom interface argument must be an interface type",
            messageFormat: "Class '{0}' is decorated with [Singleton(typeof({1}))] but '{1}' is not an interface",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CustomInterfaceMustBeExtensible = new DiagnosticDescriptor(
            id: "ERG0009",
            title: "[Singleton] custom interface must be partial",
            messageFormat: "Mark interface '{1}' as partial so the [Singleton] generator can extend it",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DuplicateCustomInterface = new DiagnosticDescriptor(
            id: "ERG0010",
            title: "[Singleton] interface already owned by another class",
            messageFormat: "Class '{0}' shares singleton interface '{1}' with another [Singleton] class. Only one class can own a given singleton interface.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
