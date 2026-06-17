namespace ButterMorph.Json.Schema;

/// <summary>
/// Defines export behavior from ButterMorph schemas to JSON Schema text.
/// </summary>
public interface IJsonSchemaExporter
{
    /// <summary>
    /// Exports a ButterMorph schema.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The conversion result.</returns>
    JsonSchemaConversionResult Export(JsonSchemaExportRequest request);
}
