namespace ButterMorph.Language;

using ButterMorph.SchemaDesign;

/// <summary>
/// Generates language-specific code from ButterMorph payload schemas.
/// </summary>
public interface IButterMorphLanguageAdapter
{
    /// <summary>
    /// Gets the adapter key.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets the language display name.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the default file extension for generated code.
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Gets the file extensions supported when exporting generated code.
    /// </summary>
    IReadOnlyCollection<string> ExportFileExtensions => string.IsNullOrWhiteSpace(FileExtension) ? [] : [FileExtension];

    /// <summary>
    /// Gets the language-native types available to mapping profiles.
    /// </summary>
    /// <returns>The language type catalog.</returns>
    IReadOnlyCollection<ButterMorphLanguageType> GetLanguageTypes();

    /// <summary>
    /// Creates a default mapping profile for a schema.
    /// </summary>
    /// <param name="definition">The schema definition when available.</param>
    /// <param name="rawJson">The raw JSON schema.</param>
    /// <returns>The default profile.</returns>
    ButterMorphLanguageMappingProfile CreateDefaultProfile(PayloadSchemaDefinition definition, string rawJson);

    /// <summary>
    /// Generates code for a schema using a language mapping profile.
    /// </summary>
    /// <param name="definition">The schema definition when available.</param>
    /// <param name="rawJson">The raw JSON schema.</param>
    /// <param name="profile">The mapping profile.</param>
    /// <returns>The generated code result.</returns>
    ButterMorphCodeGenerationResult GenerateCode(PayloadSchemaDefinition definition, string rawJson, ButterMorphLanguageMappingProfile profile);
}
