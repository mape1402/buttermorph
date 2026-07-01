namespace ButterMorph.SchemaDesign;

/// <summary>
/// Hydrates editable schema type input from a clean schema type definition.
/// </summary>
public interface ISchemaTypeDefinitionHydrator
{
    /// <summary>
    /// Creates editable schema type input from the saved definition.
    /// </summary>
    /// <param name="definition">The saved schema type definition.</param>
    /// <returns>The editable input.</returns>
    SchemaTypeDesignInput Hydrate(SchemaTypeDefinition definition);
}
