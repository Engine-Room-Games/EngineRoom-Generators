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
    /// <summary>
    /// Source generator for [Singleton]-decorated MonoBehaviours.
    /// Injects the runtime attributes and ISingleton&lt;T&gt; interface, then for each
    /// decorated class emits a matching I&lt;ClassName&gt; interface and a partial Awake
    /// implementation that publishes the instance and (optionally) keeps it across scenes.
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public sealed class SingletonGenerator : IIncrementalGenerator
    {
        private const string AttributeFullName = "EngineRoom.SingletonAttribute";
        private const string MemberAttributeFullName = "EngineRoom.SingletonMemberAttribute";
        private const string IgnoreMemberAttributeFullName = "EngineRoom.IgnoreSingletonMemberAttribute";
        private const string MonoBehaviourFullName = "global::UnityEngine.MonoBehaviour";

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
                    AttributeFullName,
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
            var classLocation = classDeclaration.Identifier.GetLocation();
            var className = classSymbol.Name;
            var diagnostics = new List<DiagnosticInfo>();

            if (!InheritsMonoBehaviour(classSymbol))
            {
                diagnostics.Add(new DiagnosticInfo(SingletonDiagnostics.MustBeMonoBehaviour, classLocation, className));
            }

            if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                diagnostics.Add(new DiagnosticInfo(SingletonDiagnostics.MustBePartial, classLocation, className));
            }

            var existingAwake = classSymbol.GetMembers("Awake")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(static method => method.MethodKind == MethodKind.Ordinary
                    && method.Parameters.Length == 0
                    && !method.IsStatic);
            if (existingAwake is not null)
            {
                var awakeLocation = existingAwake.Locations.FirstOrDefault() ?? classLocation;
                diagnostics.Add(new DiagnosticInfo(SingletonDiagnostics.MustNotDefineAwake, awakeLocation, className));
            }

            var attribute = ctx.Attributes[0];
            var (customInterface, destroyOnLoad) = ReadAttributeArgs(attribute);

            string interfaceName;
            bool hasCustomInterface;
            if (customInterface is null)
            {
                interfaceName = "I" + className;
                hasCustomInterface = false;
            }
            else
            {
                hasCustomInterface = true;
                interfaceName = customInterface.ToDisplayString(SymbolFormatter.FullyQualifiedType);

                if (customInterface.TypeKind != TypeKind.Interface)
                {
                    diagnostics.Add(new DiagnosticInfo(
                        SingletonDiagnostics.CustomInterfaceMustBeInterface,
                        classLocation,
                        className,
                        customInterface.ToDisplayString()));
                }
                else if (!ClassListsInterface(classSymbol, customInterface))
                {
                    diagnostics.Add(new DiagnosticInfo(
                        SingletonDiagnostics.CustomInterfaceNotImplemented,
                        classLocation,
                        className,
                        customInterface.ToDisplayString()));
                }
            }

            var memberDeclarations = CollectMembers(classSymbol, diagnostics, classLocation);

            var containingNamespace = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString();
            var hintPrefix = containingNamespace is null ? className : containingNamespace + "." + className;

            return new SingletonInfo(
                className: className,
                interfaceName: interfaceName,
                hasCustomInterface: hasCustomInterface,
                @namespace: containingNamespace,
                hintPrefix: hintPrefix,
                destroyOnLoad: destroyOnLoad,
                memberDeclarations: memberDeclarations,
                diagnostics: diagnostics);
        }

        private static List<string> CollectMembers(INamedTypeSymbol classSymbol, List<DiagnosticInfo> diagnostics, Location fallbackLocation)
        {
            var members = classSymbol.GetMembers();
            var hasExplicitTag = members.Any(static member => HasAttribute(member, MemberAttributeFullName));

            return hasExplicitTag
                ? CollectExplicitMembers(members, diagnostics, fallbackLocation)
                : CollectAutoMembers(classSymbol, members, fallbackLocation);
        }

        private static List<string> CollectExplicitMembers(ImmutableArray<ISymbol> members, List<DiagnosticInfo> diagnostics, Location fallbackLocation)
        {
            var declarations = new List<string>();

            foreach (var member in members)
            {
                var memberLocation = member.Locations.FirstOrDefault() ?? fallbackLocation;

                if (HasAttribute(member, IgnoreMemberAttributeFullName))
                {
                    diagnostics.Add(new DiagnosticInfo(SingletonDiagnostics.IgnoreUnusedInExplicitMode, memberLocation, member.Name));
                }

                if (!HasAttribute(member, MemberAttributeFullName))
                {
                    continue;
                }

                if (member.IsStatic)
                {
                    diagnostics.Add(new DiagnosticInfo(SingletonDiagnostics.MemberMustBeInstance, memberLocation, member.Name));
                    continue;
                }

                if (member.DeclaredAccessibility != Accessibility.Public)
                {
                    diagnostics.Add(new DiagnosticInfo(SingletonDiagnostics.MemberMustBePublic, memberLocation, member.Name));
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

        private static List<string> CollectAutoMembers(INamedTypeSymbol classSymbol, ImmutableArray<ISymbol> members, Location fallbackLocation)
        {
            _ = fallbackLocation;
            var declarations = new List<string>();

            foreach (var member in members)
            {
                if (HasAttribute(member, IgnoreMemberAttributeFullName))
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

        private static bool HasAttribute(ISymbol member, string attributeFullName)
        {
            foreach (var attribute in member.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() == attributeFullName)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ClassListsInterface(INamedTypeSymbol classSymbol, INamedTypeSymbol interfaceSymbol)
        {
            foreach (var declared in classSymbol.Interfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(declared, interfaceSymbol))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool InheritsMonoBehaviour(INamedTypeSymbol type)
        {
            var current = type.BaseType;
            while (current is not null)
            {
                var name = current.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (name == MonoBehaviourFullName)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        private static (INamedTypeSymbol? CustomInterface, bool DestroyOnLoad) ReadAttributeArgs(AttributeData attribute)
        {
            INamedTypeSymbol? customInterface = null;
            bool destroyOnLoad = false;

            // Ctor overloads are (bool) and (Type, bool); dispatch by argument kind
            // so the order of detection is independent of which overload was picked.
            foreach (var arg in attribute.ConstructorArguments)
            {
                if (arg.Kind == TypedConstantKind.Type && arg.Value is INamedTypeSymbol type)
                {
                    customInterface = type;
                }
                else if (arg.Value is bool flag)
                {
                    destroyOnLoad = flag;
                }
            }

            foreach (var pair in attribute.NamedArguments)
            {
                if (pair.Key == "DestroyOnLoad" && pair.Value.Value is bool flag)
                {
                    destroyOnLoad = flag;
                }
                else if (pair.Key == "Interface" && pair.Value.Value is INamedTypeSymbol type)
                {
                    customInterface = type;
                }
            }

            return (customInterface, destroyOnLoad);
        }

        private static void AddRuntimeType(IncrementalGeneratorPostInitializationContext ctx, string templateName)
        {
            var body = TemplateLoader.Load("Singleton/" + templateName);
            var source = SourceFileBuilder.Build(body, RuntimeNamespace);
            ctx.AddSource(templateName + ".g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private static void Emit(SourceProductionContext ctx, SingletonInfo info)
        {
            foreach (var diagnostic in info.Diagnostics)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(diagnostic.Descriptor, diagnostic.Location, diagnostic.Args));
            }

            if (info.HasBlockingDiagnostic)
            {
                return;
            }

            // Substitution drops in just the call; the placeholder already sits at
            // the right indent inside the Awake body in the template.
            var dontDestroyLine = info.DestroyOnLoad
                ? string.Empty
                : "DontDestroyOnLoad(gameObject);";

            // The user adds the custom interface to the base list themselves, so the
            // generated partial declares the class with no base list of its own.
            var baseList = info.HasCustomInterface ? string.Empty : " : " + info.InterfaceName;

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
                return;
            }

            // Member declarations land inside the interface block, so indent each
            // line to match the type body's "members live one level in" convention.
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
