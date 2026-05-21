using System.Collections.Generic;

namespace EngineRoom.Generators.Singleton
{
    internal sealed class SingletonInfo
    {
        public SingletonInfo(
            string className,
            string interfaceName,
            bool hasCustomInterface,
            string? customInterfaceShortName,
            string customInterfaceAccessibility,
            string? customInterfaceNamespace,
            string? @namespace,
            string hintPrefix,
            bool destroyOnLoad,
            IReadOnlyList<string> memberDeclarations)
        {
            ClassName = className;
            InterfaceName = interfaceName;
            HasCustomInterface = hasCustomInterface;
            CustomInterfaceShortName = customInterfaceShortName;
            CustomInterfaceAccessibility = customInterfaceAccessibility;
            CustomInterfaceNamespace = customInterfaceNamespace;
            Namespace = @namespace;
            HintPrefix = hintPrefix;
            DestroyOnLoad = destroyOnLoad;
            MemberDeclarations = memberDeclarations;
        }

        public string ClassName { get; }

        public string InterfaceName { get; }

        public bool HasCustomInterface { get; }

        public string? CustomInterfaceShortName { get; }

        public string CustomInterfaceAccessibility { get; }

        public string? CustomInterfaceNamespace { get; }

        public string? Namespace { get; }

        public string HintPrefix { get; }

        public bool DestroyOnLoad { get; }

        public IReadOnlyList<string> MemberDeclarations { get; }
    }
}
