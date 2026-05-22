using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using EngineRoom.Generators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EngineRoom.Generators.Singleton
{
    // Owns every diagnostic in the ERG00xx range. SingletonGenerator silently
    // skips invalid input, so every user-facing error/warning surfaces from here.
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SingletonAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            SingletonDiagnostics.MustBeMonoBehaviour,
            SingletonDiagnostics.MustBePartial,
            SingletonDiagnostics.MustNotDefineAwake,
            SingletonDiagnostics.MemberMustBePublic,
            SingletonDiagnostics.MemberMustBeInstance,
            SingletonDiagnostics.IgnoreUnusedInExplicitMode,
            SingletonDiagnostics.CustomInterfaceNotImplemented,
            SingletonDiagnostics.CustomInterfaceMustBeInterface,
            SingletonDiagnostics.CustomInterfaceMustBeExtensible,
            SingletonDiagnostics.DuplicateCustomInterface);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Per-compilation state — an analyzer instance is reused across
            // compilations, and the collision tracker must not leak between them.
            context.RegisterCompilationStartAction(static start =>
            {
                var collisionTracker = new InterfaceCollisionTracker();

                start.RegisterSymbolAction(
                    ctx => AnalyzeNamedType(ctx, collisionTracker),
                    SymbolKind.NamedType);

                start.RegisterCompilationEndAction(collisionTracker.Report);
            });
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext ctx, InterfaceCollisionTracker collisionTracker)
        {
            if (ctx.Symbol is not INamedTypeSymbol classSymbol || classSymbol.TypeKind != TypeKind.Class)
            {
                return;
            }

            var singletonAttribute = classSymbol.GetAttributes().FirstOrDefault(static attr =>
                attr.AttributeClass?.ToDisplayString() == SingletonConstants.AttributeFullName);
            if (singletonAttribute is null)
            {
                return;
            }

            var classLocation = GetClassIdentifierLocation(classSymbol) ?? Location.None;
            var className = classSymbol.Name;

            if (!SymbolInspector.InheritsFrom(classSymbol, SingletonConstants.MonoBehaviourFullyQualifiedName))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(SingletonDiagnostics.MustBeMonoBehaviour, classLocation, className));
            }

            if (!HasPartialDeclaration(classSymbol))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(SingletonDiagnostics.MustBePartial, classLocation, className));
            }

            var existingAwake = classSymbol.GetMembers("Awake")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(static method => method.MethodKind == MethodKind.Ordinary
                    && method.Parameters.Length == 0
                    && !method.IsStatic);
            if (existingAwake is not null)
            {
                var awakeLocation = existingAwake.Locations.FirstOrDefault() ?? classLocation;
                ctx.ReportDiagnostic(Diagnostic.Create(SingletonDiagnostics.MustNotDefineAwake, awakeLocation, className));
            }

            var (customInterface, _) = SingletonAttributeReader.Read(singletonAttribute);
            if (customInterface is not null)
            {
                ValidateCustomInterface(ctx, classSymbol, customInterface, classLocation, className);

                // Two classes targeting the same custom interface would race on
                // the shared static Instance slot — flag at compilation-end.
                collisionTracker.Record(classSymbol, customInterface, classLocation);
            }

            ValidateMembers(ctx, classSymbol);
        }

        private static void ValidateCustomInterface(
            SymbolAnalysisContext ctx,
            INamedTypeSymbol classSymbol,
            INamedTypeSymbol customInterface,
            Location classLocation,
            string className)
        {
            var interfaceDisplay = customInterface.ToDisplayString();

            if (customInterface.TypeKind != TypeKind.Interface)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    SingletonDiagnostics.CustomInterfaceMustBeInterface,
                    classLocation,
                    className,
                    interfaceDisplay));
                return;
            }

            if (!SymbolInspector.ImplementsInterface(classSymbol, customInterface))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    SingletonDiagnostics.CustomInterfaceNotImplemented,
                    classLocation,
                    className,
                    interfaceDisplay));
            }

            if (!SymbolInspector.IsTopLevelPartialInCompilation(customInterface))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    SingletonDiagnostics.CustomInterfaceMustBeExtensible,
                    classLocation,
                    className,
                    interfaceDisplay));
            }
        }

        private static void ValidateMembers(SymbolAnalysisContext ctx, INamedTypeSymbol classSymbol)
        {
            var members = classSymbol.GetMembers();
            var hasExplicitTag = members.Any(static member =>
                SymbolInspector.HasAttribute(member, SingletonConstants.IncludeAttributeFullName));

            foreach (var member in members)
            {
                var memberLocation = member.Locations.FirstOrDefault() ?? Location.None;

                if (hasExplicitTag
                    && SymbolInspector.HasAttribute(member, SingletonConstants.IgnoreAttributeFullName))
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        SingletonDiagnostics.IgnoreUnusedInExplicitMode,
                        memberLocation,
                        member.Name));
                }

                if (!SymbolInspector.HasAttribute(member, SingletonConstants.IncludeAttributeFullName))
                {
                    continue;
                }

                if (member.IsStatic)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        SingletonDiagnostics.MemberMustBeInstance,
                        memberLocation,
                        member.Name));
                    continue;
                }

                if (member.DeclaredAccessibility != Accessibility.Public)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        SingletonDiagnostics.MemberMustBePublic,
                        memberLocation,
                        member.Name));
                }
            }
        }

        private static bool HasPartialDeclaration(INamedTypeSymbol classSymbol)
        {
            foreach (var reference in classSymbol.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is ClassDeclarationSyntax declaration
                    && declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    return true;
                }
            }

            return false;
        }

        private static Location? GetClassIdentifierLocation(INamedTypeSymbol classSymbol)
        {
            foreach (var reference in classSymbol.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is ClassDeclarationSyntax declaration)
                {
                    return declaration.Identifier.GetLocation();
                }
            }

            return classSymbol.Locations.FirstOrDefault();
        }

        private sealed class InterfaceCollisionTracker
        {
            // Concurrent because RegisterSymbolAction fires in parallel when
            // EnableConcurrentExecution is set.
            private readonly ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<ClaimingClass>> _claims =
                new ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<ClaimingClass>>(SymbolEqualityComparer.Default);

            public void Record(INamedTypeSymbol classSymbol, INamedTypeSymbol customInterface, Location classLocation)
            {
                var bag = _claims.GetOrAdd(customInterface, static _ => new ConcurrentBag<ClaimingClass>());
                bag.Add(new ClaimingClass(classSymbol.Name, classLocation));
            }

            public void Report(CompilationAnalysisContext ctx)
            {
                foreach (var pair in _claims)
                {
                    if (pair.Value.Count < 2)
                    {
                        continue;
                    }

                    var interfaceDisplay = pair.Key.ToDisplayString();
                    foreach (var claim in pair.Value)
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(
                            SingletonDiagnostics.DuplicateCustomInterface,
                            claim.Location,
                            claim.ClassName,
                            interfaceDisplay));
                    }
                }
            }

            private readonly struct ClaimingClass
            {
                public ClaimingClass(string className, Location location)
                {
                    ClassName = className;
                    Location = location;
                }

                public string ClassName { get; }

                public Location Location { get; }
            }
        }
    }
}
