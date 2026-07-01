namespace ButterMorph.SchemaDesign;

/// <summary>
/// Hydrates editable field metadata input from a clean custom field definition.
/// </summary>
public interface IFieldMetadataDefinitionHydrator
{
    /// <summary>
    /// Creates editable field metadata input from the saved definition.
    /// </summary>
    /// <param name="definition">The saved custom field definition.</param>
    /// <returns>The editable input.</returns>
    FieldMetadataDesignInput Hydrate(CustomFieldDefinition definition);
}
