using System;

namespace EngineRoom.Generators.Dependency
{
    // Strings-only so the incremental cache key doesn't depend on Roslyn symbol
    // identity (which churns between compilations).
    internal readonly struct DependencyFieldEntry : IEquatable<DependencyFieldEntry>
    {
        public DependencyFieldEntry(
            string classFullName,
            string? @namespace,
            string className,
            string hintPrefix,
            string fieldName,
            string fieldTypeText)
        {
            ClassFullName = classFullName;
            Namespace = @namespace;
            ClassName = className;
            HintPrefix = hintPrefix;
            FieldName = fieldName;
            FieldTypeText = fieldTypeText;
        }

        public string ClassFullName { get; }

        public string? Namespace { get; }

        public string ClassName { get; }

        public string HintPrefix { get; }

        public string FieldName { get; }

        public string FieldTypeText { get; }

        public bool Equals(DependencyFieldEntry other)
        {
            return ClassFullName == other.ClassFullName
                && Namespace == other.Namespace
                && ClassName == other.ClassName
                && HintPrefix == other.HintPrefix
                && FieldName == other.FieldName
                && FieldTypeText == other.FieldTypeText;
        }

        public override bool Equals(object? obj)
        {
            return obj is DependencyFieldEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (ClassFullName?.GetHashCode() ?? 0);
                hash = hash * 31 + (Namespace?.GetHashCode() ?? 0);
                hash = hash * 31 + (ClassName?.GetHashCode() ?? 0);
                hash = hash * 31 + (HintPrefix?.GetHashCode() ?? 0);
                hash = hash * 31 + (FieldName?.GetHashCode() ?? 0);
                hash = hash * 31 + (FieldTypeText?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
