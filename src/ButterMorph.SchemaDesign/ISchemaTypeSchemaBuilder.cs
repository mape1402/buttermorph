namespace ButterMorph.SchemaDesign;

/// <summary>
/// Builds JSON Schema for custom schema type versions.
/// </summary>
public interface ISchemaTypeSchemaBuilder
{
    /// <summary>
    /// Builds the schema type result.
    /// </summary>
    /// <param name="input">The design input.</param>
    /// <param name="catalog">The available schema types.</param>
    /// <returns>The design result.</returns>
    SchemaTypeDesignResult Build(SchemaTypeDesignInput input, IReadOnlyCollection<SchemaTypeCatalogItem> catalog);
}