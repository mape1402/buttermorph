namespace ButterMorph.SchemaDesign;

using System.Text;
using System.Text.Json;

/// <summary>
/// Provides shared JSON helpers for schema design.
/// </summary>
internal static class SchemaDesignJsonTools
{
    // Keeps JSON payloads compact and deterministic.
    private static readonly JsonWriterOptions CompactOptions = new()
    {
        Indented = false
    };

    /// <summary>
    /// Compacts JSON text while preserving its semantic value.
    /// </summary>
    /// <param name="json">The JSON text.</param>
    /// <returns>The compact JSON text.</returns>
    internal static string Compact(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        using JsonDocument document = JsonDocument.Parse(json);
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, CompactOptions);
        document.RootElement.WriteTo(writer);
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Reads newline-delimited values into a unique collection.
    /// </summary>
    /// <param name="text">The newline-delimited text.</param>
    /// <returns>The normalized values.</returns>
    internal static IReadOnlyCollection<string> ReadLines(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => value.Trim('\r'))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}