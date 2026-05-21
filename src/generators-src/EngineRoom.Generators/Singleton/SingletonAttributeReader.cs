using Microsoft.CodeAnalysis;

namespace EngineRoom.Generators.Singleton
{
    internal static class SingletonAttributeReader
    {
        public static (INamedTypeSymbol? CustomInterface, bool DestroyOnLoad) Read(AttributeData attribute)
        {
            INamedTypeSymbol? customInterface = null;
            bool destroyOnLoad = false;

            // Ctor overloads are (bool) and (Type, bool); dispatch by argument
            // Kind so detection works regardless of which overload was picked.
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
    }
}
