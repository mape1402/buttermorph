namespace ButterMorph.SchemaDesign;

/// <summary>
/// Hydrates editable payload schema input from a clean payload schema definition.
/// </summary>
public interface IPayloadSchemaDefinitionHydrator
{
    /// <summary>
    /// Creates editable payload schema input from the saved definition.
    /// </summary>
    /// <param name="definition">The saved payload schema definition.</param>
    /// <returns>The editable input.</returns>
    PayloadSchemaDesignInput Hydrate(PayloadSchemaDefinition definition);
}
