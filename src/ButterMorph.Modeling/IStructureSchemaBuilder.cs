namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;

/// <summary>
/// Builds structure schemas.
/// </summary>
public interface IStructureSchemaBuilder
{
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
