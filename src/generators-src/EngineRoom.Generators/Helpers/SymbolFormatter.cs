using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace EngineRoom.Generators.Helpers
{
    /// <summary>
    /// Renders Roslyn symbols into source-text fragments suitable for embedding in
    /// generated code (interface declarations, today; extendable to other shapes
    /// as more generators land).
    /// </summary>
    internal static class SymbolFormatter
    {
        public static readonly SymbolDisplayFormat FullyQualifiedType = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
                | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        public static string AccessibilityKeyword(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Public => "public ",
                Accessibility.Internal => "internal ",
                Accessibility.Protected => "protected ",
                Accessibility.ProtectedAndInternal => "private protected ",
                Accessibility.ProtectedOrInternal => "protected internal ",
                Accessibility.Private => "private ",
                _ => string.Empty,
            };
        }

        public static string FormatAsInterfaceMember(ISymbol symbol)
        {
            return symbol switch
            {
                IMethodSymbol method => FormatMethod(method),
                IPropertySymbol property => FormatProperty(property),
                _ => string.Empty,
            };
        }

        private static string FormatMethod(IMethodSymbol method)
        {
            var builder = new StringBuilder();
            builder.Append(method.ReturnType.ToDisplayString(FullyQualifiedType));
            builder.Append(' ');
            builder.Append(method.Name);

            if (method.TypeParameters.Length > 0)
            {
                builder.Append('<');
                builder.Append(string.Join(", ", method.TypeParameters.Select(static parameter => parameter.Name)));
                builder.Append('>');
            }

            builder.Append('(');
            builder.Append(string.Join(", ", method.Parameters.Select(FormatParameter)));
            builder.Append(");");

            return builder.ToString();
        }

        private static string FormatProperty(IPropertySymbol property)
        {
            var builder = new StringBuilder();
            builder.Append(property.Type.ToDisplayString(FullyQualifiedType));
            builder.Append(' ');
            builder.Append(property.Name);
            builder.Append(" { ");

            if (property.GetMethod is not null && property.GetMethod.DeclaredAccessibility == Accessibility.Public)
            {
                builder.Append("get; ");
            }

            if (property.SetMethod is not null && property.SetMethod.DeclaredAccessibility == Accessibility.Public)
            {
                builder.Append("set; ");
            }

            builder.Append('}');
            return builder.ToString();
        }

        private static string FormatParameter(IParameterSymbol parameter)
        {
            var builder = new StringBuilder();

            switch (parameter.RefKind)
            {
                case RefKind.Ref:
                    builder.Append("ref ");
                    break;
                case RefKind.Out:
                    builder.Append("out ");
                    break;
                case RefKind.In:
                    builder.Append("in ");
                    break;
            }

            if (parameter.IsParams)
            {
                builder.Append("params ");
            }

            builder.Append(parameter.Type.ToDisplayString(FullyQualifiedType));
            builder.Append(' ');
            builder.Append(parameter.Name);

            if (parameter.HasExplicitDefaultValue)
            {
                builder.Append(" = ");
                builder.Append(FormatDefaultValue(parameter.ExplicitDefaultValue));
            }

            return builder.ToString();
        }

        private static string FormatDefaultValue(object? value)
        {
            return value switch
            {
                null => "default",
                string text => "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
                char character => "'" + character + "'",
                bool boolean => boolean ? "true" : "false",
                _ => value.ToString() ?? "default",
            };
        }
    }
}
