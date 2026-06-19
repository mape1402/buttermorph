namespace ButterMorph.SchemaDesign;

/// <summary>
/// Represents payload schema builder input.
/// </summary>
public sealed class PayloadSchemaDesignInput
{
    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string Name { get; set; } = "Payload";

    /// <summary>
    /// Gets or sets the payload JSON Schema.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;
}