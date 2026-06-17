namespace ButterMorph.Json.Schema;

/// <summary>
/// Defines import behavior from JSON Schema text to ButterMorph schemas.
/// </summary>
public interface IJsonSchemaImporter
{
    /// <summary>
    /// Imports a JSON Schema document.
    /// </summary>
    /// <param name="request">The import request.</param>
    /// <returns>The conversion result.</returns>
    JsonSchemaConversionResult Import(JsonSchemaImportRequest request);
}
