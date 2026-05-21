using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EngineRoom.Generators.Helpers
{
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

        // A generator can safely emit a partial extending this type only when it's
        // top-level, non-generic, and partial in the current compilation — otherwise
        // the emitted partial won't merge with the user's declaration.
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
