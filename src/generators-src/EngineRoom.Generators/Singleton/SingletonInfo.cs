using System.Collections.Generic;

namespace EngineRoom.Generators.Singleton
{
    internal sealed class SingletonInfo
    {
        public SingletonInfo(
            string className,
            string interfaceName,
            bool hasCustomInterface,
            string? @namespace,
            string hintPrefix,
            bool destroyOnLoad,
            IReadOnlyList<string> memberDeclarations,
            IReadOnlyList<DiagnosticInfo> diagnostics)
        {
            ClassName = className;
            InterfaceName = interfaceName;
            HasCustomInterface = hasCustomInterface;
            Namespace = @namespace;
            HintPrefix = hintPrefix;
            DestroyOnLoad = destroyOnLoad;
            MemberDeclarations = memberDeclarations;
            Diagnostics = diagnostics;
        }

        public string ClassName { get; }

        public string InterfaceName { get; }

        public bool HasCustomInterface { get; }

        public string? Namespace { get; }

        public string HintPrefix { get; }

        public bool DestroyOnLoad { get; }

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
                        || id == SingletonDiagnostics.CustomInterfaceMustBeInterface.Id)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
