using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EngineRoom.Generators.Helpers
{
    internal static class TemplateLoader
    {
        private const string ResourceNamespace = "EngineRoom.Generators.Templates.";
        private const string ResourceExtension = ".cs.txt";

        private static readonly Assembly TemplateAssembly = typeof(TemplateLoader).Assembly;

        public static string Load(string templateName)
        {
            // Embedded-resource manifest names use dots as separators, not slashes.
            var resourcePath = templateName.Replace('/', '.');
            var resourceName = ResourceNamespace + resourcePath + ResourceExtension;
            using var stream = TemplateAssembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                throw new InvalidOperationException($"Embedded template not found: {resourceName}");
            }

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static string LoadAndSubstitute(string templateName, IReadOnlyDictionary<string, string> placeholders)
        {
            var text = Load(templateName);
            return placeholders.Aggregate(text, (current, pair) => current.Replace($"%%{pair.Key}%%", pair.Value));
        }
    }
}
