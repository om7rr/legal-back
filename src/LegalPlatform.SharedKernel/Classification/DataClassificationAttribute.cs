namespace LegalPlatform.SharedKernel.Classification;

/// <summary>
/// Tags an entity type with its <see cref="DataClassification"/>. Code review blocks any
/// new persisted entity that lacks this attribute (see docs/security/checklist.md).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class DataClassificationAttribute : Attribute
{
    public DataClassificationAttribute(DataClassification classification)
    {
        Classification = classification;
    }

    public DataClassification Classification { get; }
}
