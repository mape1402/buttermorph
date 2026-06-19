namespace ButterMorph.SchemaDesign;

/// <summary>
/// Builds field metadata definitions.
/// </summary>
public interface IFieldMetadataDefinitionBuilder
{
    /// <summary>
    /// Builds the field metadata result.
    /// </summary>
    /// <param name="input">The design input.</param>
    /// <returns>The design result.</returns>
    FieldMetadataDesignResult Build(FieldMetadataDesignInput input);
}