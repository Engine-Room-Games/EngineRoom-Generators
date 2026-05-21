using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EngineRoom.Generators.Helpers
{
    /// <summary>
    /// Read-only inspection helpers for Roslyn symbols. Distinct from
    /// <see cref="SymbolFormatter"/>, which renders symbols into source text —
    /// these methods only answer questions about symbols.
    /// </summary>
    internal static class SymbolInspector
    {
        public static bool HasAttribute(ISymbol symbol, string attributeFullName)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() == attributeFullName)
                {
                    return true;
                }
            }

            return false;
        }

        // Walks the base chain; the comparison is by fully-qualified name so the
        // caller can pass a "global::UnityEngine.MonoBehaviour"-style string
        // without needing the actual INamedTypeSymbol in hand.
        public static bool InheritsFrom(INamedTypeSymbol type, string baseTypeFullyQualifiedName)
        {
            var current = type.BaseType;
            while (current is not null)
            {
                if (current.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == baseTypeFullyQualifiedName)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        public static bool ImplementsInterface(INamedTypeSymbol type, INamedTypeSymbol interfaceSymbol)
        {
            foreach (var declared in type.Interfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(declared, interfaceSymbol))
                {
                    return true;
                }
            }

            return false;
        }

        // True when a generator can safely emit a partial that extends this type:
        // top-level (not nested), non-generic, and declared as a partial in the
        // current compilation. Without these, the emitted partial wouldn't merge
        // with the user's declaration.
        public static bool IsTopLevelPartialInCompilation(INamedTypeSymbol type)
        {
            if (type.ContainingType is not null
                || type.IsGenericType
                || type.DeclaringSyntaxReferences.Length == 0)
            {
                return false;
            }

            foreach (var reference in type.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is BaseTypeDeclarationSyntax declaration
                    && declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
