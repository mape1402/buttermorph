namespace ButterMorph.Language;

/// <summary>
/// Describes code generation settings and type mappings for one target language.
/// </summary>
public sealed class ButterMorphLanguageMappingProfile
{
    private string key = string.Empty;
    private string language = string.Empty;

    /// <summary>
    /// Gets or sets the host/profile identifier.
    /// </summary>
    public string Id
    {
        get => key;
        set => key = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the stable package key.
    /// </summary>
    public string Key
    {
        get => key;
        set => key = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the package description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language adapter key.
    /// </summary>
    public string LanguageKey
    {
        get => language;
        set => language = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the package target language.
    /// </summary>
    public string Language
    {
        get => language;
        set => language = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the namespace or package name.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the root generated type name.
    /// </summary>
    public string RootTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether traceability attributes should be emitted.
    /// </summary>
    public bool EmitAttributes { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether nullable reference type syntax should be emitted.
    /// </summary>
    public bool UseNullableReferences { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether C# required members should be emitted for required fields.
    /// </summary>
    public bool UseRequiredMembers { get; set; } = true;

    /// <summary>
    /// Gets or sets the generic collection type used for arrays.
    /// </summary>
    public string CollectionType { get; set; } = "List";

    /// <summary>
    /// Gets or sets type mappings for this profile.
    /// </summary>
    public List<ButterMorphLanguageTypeMapping> TypeMappings { get; set; } = [];

    /// <summary>
    /// Gets or sets optional schema-field to member-name mappings for generated code.
    /// </summary>
    public List<ButterMorphLanguageMemberMapping> MemberMappings { get; set; } = [];

    /// <summary>
    /// Gets or sets optional schema-field to generated type-name mappings for generated code.
    /// </summary>
    public List<ButterMorphLanguageTypeNameMapping> TypeNameMappings { get; set; } = [];
}
