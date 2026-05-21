using Microsoft.CodeAnalysis;

namespace EngineRoom.Generators.Helpers
{
    /// <summary>
    /// Deferred diagnostic carrier. Collected during the syntax-provider transform
    /// (which must stay value-equatable for incremental caching) and converted to
    /// real <see cref="Diagnostic"/> instances at the source-output stage.
    /// </summary>
    internal sealed class DiagnosticInfo
    {
        public DiagnosticInfo(DiagnosticDescriptor descriptor, Location location, params object[] args)
        {
            Descriptor = descriptor;
            Location = location;
            Args = args;
        }

        public DiagnosticDescriptor Descriptor { get; }

        public Location Location { get; }

        public object[] Args { get; }
    }
}
