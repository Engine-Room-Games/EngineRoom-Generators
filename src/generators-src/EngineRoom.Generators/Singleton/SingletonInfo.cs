using System.Collections.Generic;
using Microsoft.CodeAnalysis;

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
            Location classLocation,
            IReadOnlyList<string> memberDeclarations,
            IReadOnlyList<DiagnosticInfo> diagnostics)
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
            ClassLocation = classLocation;
            MemberDeclarations = memberDeclarations;
            Diagnostics = diagnostics;
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

        public Location ClassLocation { get; }

        public IReadOnlyList<string> MemberDeclarations { get; }

        public IReadOnlyList<DiagnosticInfo> Diagnostics { get; }

        public bool HasBlockingDiagnostic
        {
            get
            {
                foreach (var diagnostic in Diagnostics)
                {
                    var id = diagnostic.Descriptor.Id;
                    if (id == SingletonDiagnostics.MustBeMonoBehaviour.Id
                        || id == SingletonDiagnostics.MustBePartial.Id
                        || id == SingletonDiagnostics.MustNotDefineAwake.Id
                        || id == SingletonDiagnostics.CustomInterfaceNotImplemented.Id
                        || id == SingletonDiagnostics.CustomInterfaceMustBeInterface.Id
                        || id == SingletonDiagnostics.CustomInterfaceMustBeExtensible.Id)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
