namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;

/// <summary>
/// Builds structure schemas.
/// </summary>
public interface IStructureSchemaBuilder
{
    /// <summary>
    /// Sets the canonical schema key.
    /// </summary>
    /// <param name="key">The schema key.</param>
    /// <returns>The current builder.</returns>
    IStructureSchemaBuilder WithKey(string key);

    /// <summary>
    /// Sets the schema description.
    /// </summary>
    /// <param name="description">The schema description.</param>
    /// <returns>The current builder.</returns>
    IStructureSchemaBuilder WithDescription(string description);

    /// <summary>
    /// Sets the schema version.
    /// </summary>
    /// <param name="version">The schema version.</param>
    /// <returns>The current builder.</returns>
    IStructureSchemaBuilder WithVersion(string version);

    /// <summary>
    /// Sets the schema version comment.
    /// </summary>
    /// <param name="versionComment">The schema version comment.</param>
    /// <returns>The current builder.</returns>
    IStructureSchemaBuilder WithVersionComment(string versionComment);

    /// <summary>
    /// Sets the schema root.
    /// </summary>
    /// <param name="root">The schema root.</param>
    /// <returns>The current builder.</returns>
    IStructureSchemaBuilder WithRoot(ISchemaNode root);

    /// <summary>
    /// Adds schema metadata.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current builder.</returns>
    IStructureSchemaBuilder WithMetadata(string key, string value);

    /// <summary>
    /// Builds the structure schema.
    /// </summary>
    /// <returns>The structure schema.</returns>
    IStructureSchema Build();
}
