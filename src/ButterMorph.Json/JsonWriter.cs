namespace ButterMorph.Json;

using System.Text;
using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Writes internal structure graphs to JSON output.
/// </summary>
public sealed class JsonWriter : IStructureWriter
{
    /// <summary>
    /// Writes a structure graph to JSON output.
    /// </summary>
    /// <param name="graph">The internal structure graph.</param>
    /// <returns>The JSON structure output.</returns>
    public IStructureOutput Write(IStructureGraph graph)
    {
        return new StructureOutput
        {
            Format = "json",
            Content = MapToJson(graph.Root)
        };
    }

    // Converts the internal graph root node into serialized JSON content.
    private static string MapToJson(IStructureNode node)
    {
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);

        WriteNode(writer, node);
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Writes a graph node using the JSON shape represented by the node kind.
    private static void WriteNode(Utf8JsonWriter writer, IStructureNode node)
    {
        switch (node.Kind)
        {
            case StructureNodeKind.Object:
                WriteObject(writer, node);
                break;
            case StructureNodeKind.Array:
                WriteArray(writer, node);
                break;
            case StructureNodeKind.Scalar:
                WriteScalar(writer, (IScalarStructureNode)node);
                break;
            default:
                throw new InvalidOperationException("Unsupported structure node kind.");
        }
    }

    // Writes Object nodes as JSON maps.
    private static void WriteObject(Utf8JsonWriter writer, IStructureNode node)
    {
        writer.WriteStartObject();

        foreach (IStructureNode child in node.Children)
        {
            writer.WritePropertyName(child.Name);
            WriteNode(writer, child);
        }

        writer.WriteEndObject();
    }

    // Writes Array nodes as JSON arrays.
    private static void WriteArray(Utf8JsonWriter writer, IStructureNode node)
    {
        writer.WriteStartArray();

        foreach (IStructureNode child in node.Children)
        {
            WriteNode(writer, child);
        }

        writer.WriteEndArray();
    }

    // Writes scalar nodes using their logical data type.
    private static void WriteScalar(Utf8JsonWriter writer, IScalarStructureNode node)
    {
        IScalarValue value = node.Value;

        if (value.IsNull || string.Equals(value.DataType, "Null", StringComparison.OrdinalIgnoreCase))
        {
            writer.WriteNullValue();
            return;
        }

        if (string.Equals(value.DataType, "Number", StringComparison.OrdinalIgnoreCase))
        {
            writer.WriteRawValue(value.RawValue);
            return;
        }

        if (string.Equals(value.DataType, "Boolean", StringComparison.OrdinalIgnoreCase))
        {
            writer.WriteBooleanValue(bool.Parse(value.RawValue));
            return;
        }

        writer.WriteStringValue(value.RawValue);
    }
}
