namespace ButterMorph.SchemaDesign;

/// <summary>
/// Builds payload schemas from structured field definitions.
/// </summary>
public interface IPayloadSchemaDefinitionBuilder
{
    /// <summary>
    /// Builds a payload schema result.
    /// </summary>
    /// <param name="input">The schema identity and metadata input.</param>
    /// <param name="fields">The schema fields.</param>
    /// <param name="schemaTypes">The custom type catalog.</param>
    /// <param name="metadataFields">The metadata field catalog.</param>
    /// <returns>The payload schema result.</returns>
    PayloadSchemaDesignResult Build(PayloadSchemaDesignInput input, IReadOnlyCollection<PayloadSchemaField> fields, IReadOnlyCollection<SchemaTypeCatalogItem> schemaTypes, IReadOnlyCollection<FieldMetadataCatalogItem> metadataFields);
}
