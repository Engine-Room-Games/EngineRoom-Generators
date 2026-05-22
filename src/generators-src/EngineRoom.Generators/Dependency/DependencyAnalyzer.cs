using System.Collections.Immutable;
using System.Linq;
using EngineRoom.Generators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EngineRoom.Generators.Dependency
{
    // Owns the ERG01xx range. DependencyGenerator silently skips invalid input,
    // so every user-facing error surfaces from here. Runs on the post-generator
    // compilation, which means singleton interfaces emitted by
    // SingletonGenerator are resolvable here even though they aren't visible to
    // DependencyGenerator itself.
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DependencyAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            DependencyDiagnostics.MustBeMonoBehaviour,
            DependencyDiagnostics.MustBePartial,
            DependencyDiagnostics.MustNotDefineStart,
            DependencyDiagnostics.FieldMustBeNonPublicInstance,
            DependencyDiagnostics.FieldTypeMustBeSingletonInterface);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
            context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext ctx)
        {
            if (ctx.Symbol is not INamedTypeSymbol classSymbol || classSymbol.TypeKind != TypeKind.Class)
            {
                return;
            }

            var hasDependencyField = false;
            foreach (var member in classSymbol.GetMembers())
            {
                if (member is IFieldSymbol field
                    && SymbolInspector.HasAttribute(field, DependencyConstants.AttributeFullName))
                {
                    hasDependencyField = true;
                    break;
                }
            }

            if (!hasDependencyField)
            {
                return;
            }

            var classLocation = GetClassIdentifierLocation(classSymbol) ?? Location.None;
            var className = classSymbol.Name;

            if (!SymbolInspector.InheritsFrom(classSymbol, DependencyConstants.MonoBehaviourFullyQualifiedName))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(DependencyDiagnostics.MustBeMonoBehaviour, classLocation, className));
            }

            if (!SymbolInspector.IsPartial(classSymbol))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(DependencyDiagnostics.MustBePartial, classLocation, className));
            }

            var existingStart = classSymbol.GetMembers("Start")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(static method => method.MethodKind == MethodKind.Ordinary
                    && method.Parameters.Length == 0
                    && !method.IsStatic);
            if (existingStart is not null)
            {
                var startLocation = existingStart.Locations.FirstOrDefault() ?? classLocation;
                ctx.ReportDiagnostic(Diagnostic.Create(DependencyDiagnostics.MustNotDefineStart, startLocation, className));
            }
        }

        private static void AnalyzeField(SymbolAnalysisContext ctx)
        {
            if (ctx.Symbol is not IFieldSymbol field
                || !SymbolInspector.HasAttribute(field, DependencyConstants.AttributeFullName))
            {
                return;
            }

            var location = field.Locations.FirstOrDefault() ?? Location.None;

            if (field.IsStatic
                || (field.DeclaredAccessibility != Accessibility.Private
                    && field.DeclaredAccessibility != Accessibility.Protected))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    DependencyDiagnostics.FieldMustBeNonPublicInstance,
                    location,
                    field.Name));
            }

            if (!IsSelfSingletonInterface(field.Type))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    DependencyDiagnostics.FieldTypeMustBeSingletonInterface,
                    location,
                    field.Name,
                    field.Type.ToDisplayString()));
            }
        }

        private static bool IsSelfSingletonInterface(ITypeSymbol fieldType)
        {
            if (fieldType is not INamedTypeSymbol named || named.TypeKind != TypeKind.Interface)
            {
                return false;
            }

            foreach (var iface in named.AllInterfaces)
            {
                var original = iface.OriginalDefinition;
                if (original.Arity != DependencyConstants.SingletonInterfaceArity
                    || original.Name != DependencyConstants.SingletonInterfaceName
                    || original.ContainingNamespace?.ToDisplayString() != DependencyConstants.SingletonInterfaceNamespace)
                {
                    continue;
                }

                if (SymbolEqualityComparer.Default.Equals(iface.TypeArguments[0], named))
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
    }
}
