using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using EngineRoom.Generators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EngineRoom.Generators.Singleton
{
    // Validation lives in SingletonAnalyzer; this generator silently skips any
    // input the analyzer would flag, so its output never piles on top of a
    // compile error.
    [Generator(LanguageNames.CSharp)]
    public sealed class SingletonGenerator : IIncrementalGenerator
    {
        private const string RuntimeNamespace = "EngineRoom";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static ctx =>
            {
                AddRuntimeType(ctx, "SingletonAttribute");
                AddRuntimeType(ctx, "SingletonMemberAttribute");
                AddRuntimeType(ctx, "IgnoreSingletonMemberAttribute");
                AddRuntimeType(ctx, "ISingleton");
            });

            var singletons = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    SingletonConstants.AttributeFullName,
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, _) => ExtractInfo(ctx))
                .Where(static info => info is not null);

            context.RegisterSourceOutput(singletons, static (ctx, info) => Emit(ctx, info!));
        }

        private static SingletonInfo? ExtractInfo(GeneratorAttributeSyntaxContext ctx)
        {
            if (ctx.TargetSymbol is not INamedTypeSymbol classSymbol)
            {
                return null;
            }

            var classDeclaration = (ClassDeclarationSyntax)ctx.TargetNode;

            if (!SymbolInspector.InheritsFrom(classSymbol, SingletonConstants.MonoBehaviourFullyQualifiedName))
            {
                return null;
            }

            if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return null;
            }

            var hasOwnAwake = classSymbol.GetMembers("Awake")
                .OfType<IMethodSymbol>()
                .Any(static method => method.MethodKind == MethodKind.Ordinary
                    && method.Parameters.Length == 0
                    && !method.IsStatic);
            if (hasOwnAwake)
            {
                return null;
            }

            var attribute = ctx.Attributes[0];
            var (customInterface, destroyOnLoad) = SingletonAttributeReader.Read(attribute);

            string interfaceName;
            bool hasCustomInterface;
            string? customInterfaceShortName = null;
            string customInterfaceAccessibility = string.Empty;
            string? customInterfaceNamespace = null;
            if (customInterface is null)
            {
                interfaceName = "I" + classSymbol.Name;
                hasCustomInterface = false;
            }
            else
            {
                if (customInterface.TypeKind != TypeKind.Interface
                    || !SymbolInspector.ImplementsInterface(classSymbol, customInterface)
                    || !SymbolInspector.IsTopLevelPartialInCompilation(customInterface))
                {
                    return null;
                }

                hasCustomInterface = true;
                interfaceName = customInterface.ToDisplayString(SymbolFormatter.FullyQualifiedType);
                customInterfaceShortName = customInterface.Name;
                customInterfaceAccessibility = SymbolFormatter.AccessibilityKeyword(customInterface.DeclaredAccessibility);
                customInterfaceNamespace = customInterface.ContainingNamespace.IsGlobalNamespace
                    ? null
                    : customInterface.ContainingNamespace.ToDisplayString();
            }

            var memberDeclarations = CollectMembers(classSymbol);

            var containingNamespace = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString();
            var hintPrefix = containingNamespace is null ? classSymbol.Name : containingNamespace + "." + classSymbol.Name;

            return new SingletonInfo(
                className: classSymbol.Name,
                interfaceName: interfaceName,
                hasCustomInterface: hasCustomInterface,
                customInterfaceShortName: customInterfaceShortName,
                customInterfaceAccessibility: customInterfaceAccessibility,
                customInterfaceNamespace: customInterfaceNamespace,
                @namespace: containingNamespace,
                hintPrefix: hintPrefix,
                destroyOnLoad: destroyOnLoad,
                memberDeclarations: memberDeclarations);
        }

        private static List<string> CollectMembers(INamedTypeSymbol classSymbol)
        {
            var members = classSymbol.GetMembers();
            var hasExplicitTag = members.Any(static member =>
                SymbolInspector.HasAttribute(member, SingletonConstants.MemberAttributeFullName));

            return hasExplicitTag
                ? CollectExplicitMembers(members)
                : CollectAutoMembers(classSymbol, members);
        }

        private static List<string> CollectExplicitMembers(ImmutableArray<ISymbol> members)
        {
            var declarations = new List<string>();

            foreach (var member in members)
            {
                if (!SymbolInspector.HasAttribute(member, SingletonConstants.MemberAttributeFullName))
                {
                    continue;
                }

                if (member.IsStatic || member.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                var declaration = SymbolFormatter.FormatAsInterfaceMember(member);
                if (!string.IsNullOrEmpty(declaration))
                {
                    declarations.Add(declaration);
                }
            }

            return declarations;
        }

        private static List<string> CollectAutoMembers(INamedTypeSymbol classSymbol, ImmutableArray<ISymbol> members)
        {
            var declarations = new List<string>();

            foreach (var member in members)
            {
                if (SymbolInspector.HasAttribute(member, SingletonConstants.IgnoreMemberAttributeFullName))
                {
                    continue;
                }

                if (!IsAutoIncludeCandidate(classSymbol, member))
                {
                    continue;
                }

                var declaration = SymbolFormatter.FormatAsInterfaceMember(member);
                if (!string.IsNullOrEmpty(declaration))
                {
                    declarations.Add(declaration);
                }
            }

            return declarations;
        }

        private static bool IsAutoIncludeCandidate(INamedTypeSymbol classSymbol, ISymbol member)
        {
            if (member.IsStatic
                || member.IsImplicitlyDeclared
                || member.IsOverride
                || member.DeclaredAccessibility != Accessibility.Public
                || !SymbolEqualityComparer.Default.Equals(member.ContainingType, classSymbol))
            {
                return false;
            }

            return member switch
            {
                IMethodSymbol method => method.MethodKind == MethodKind.Ordinary,
                IPropertySymbol property => !property.IsIndexer,
                _ => false,
            };
        }

        private static void AddRuntimeType(IncrementalGeneratorPostInitializationContext ctx, string templateName)
        {
            var body = TemplateLoader.Load("Singleton/" + templateName);
            var source = SourceFileBuilder.Build(body, RuntimeNamespace);
            ctx.AddSource(templateName + ".g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private static void Emit(SourceProductionContext ctx, SingletonInfo info)
        {
            var dontDestroyLine = info.DestroyOnLoad
                ? string.Empty
                : TemplateLoader.Load("Singleton/DontDestroyLine").TrimEnd('\r', '\n');

            // With a custom interface, the user already lists it in the class base
            // list themselves; the generated partial then declares the class with
            // no base list of its own to avoid a duplicate.
            var baseList = info.HasCustomInterface
                ? string.Empty
                : TemplateLoader.LoadAndSubstitute("Singleton/SingletonBaseList", new Dictionary<string, string>
                {
                    ["InterfaceName"] = info.InterfaceName,
                }).TrimEnd('\r', '\n');

            var partialBody = TemplateLoader.LoadAndSubstitute("Singleton/SingletonPartial", new Dictionary<string, string>
            {
                ["ClassName"] = info.ClassName,
                ["InterfaceName"] = info.InterfaceName,
                ["BaseList"] = baseList,
                ["DontDestroyLine"] = dontDestroyLine,
            });

            var partialText = SourceFileBuilder.Build(partialBody, info.Namespace);
            ctx.AddSource(info.HintPrefix + ".Singleton.Partial.g.cs", SourceText.From(partialText, Encoding.UTF8));

            if (info.HasCustomInterface)
            {
                // Emit a partial of the user's interface adding the ISingleton<>
                // base; without it the generated Awake's reference to Instance
                // wouldn't resolve.
                var customInterfaceBody = TemplateLoader.LoadAndSubstitute("Singleton/SingletonCustomInterfacePartial", new Dictionary<string, string>
                {
                    ["InterfaceAccessibility"] = info.CustomInterfaceAccessibility,
                    ["InterfaceShortName"] = info.CustomInterfaceShortName!,
                    ["InterfaceFullName"] = info.InterfaceName,
                });

                var customInterfaceText = SourceFileBuilder.Build(customInterfaceBody, info.CustomInterfaceNamespace);
                ctx.AddSource(info.HintPrefix + ".Singleton.CustomInterfacePartial.g.cs", SourceText.From(customInterfaceText, Encoding.UTF8));
                return;
            }

            var membersBlock = info.MemberDeclarations.Count == 0
                ? string.Empty
                : string.Join("\n", info.MemberDeclarations.Select(static line => "    " + line));

            var interfaceBody = TemplateLoader.LoadAndSubstitute("Singleton/SingletonInterface", new Dictionary<string, string>
            {
                ["InterfaceName"] = info.InterfaceName,
                ["Members"] = membersBlock,
            });

            var interfaceText = SourceFileBuilder.Build(interfaceBody, info.Namespace);
            ctx.AddSource(info.HintPrefix + ".Singleton.Interface.g.cs", SourceText.From(interfaceText, Encoding.UTF8));
        }
    }
}
