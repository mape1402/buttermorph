namespace ButterMorph.Json;

using System.Globalization;
using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Reads JSON input into the internal structure graph model.
/// </summary>
public sealed class JsonReader : IStructureReader
{
    /// <summary>
    /// Reads JSON input into a structure graph.
    /// </summary>
    /// <param name="input">The external structure input.</param>
    /// <returns>The internal structure graph.</returns>
    public IStructureGraph Read(IStructureInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.Format) && !string.Equals(input.Format, "json", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("JsonReader only supports json structure input.");
        }

        using JsonDocument document = JsonDocument.Parse(input.Content);
        return MapToGraph(document.RootElement);
    }

    // Converts the JSON root element into the internal graph representation.
    private static StructureGraph MapToGraph(JsonElement element)
    {
        List<IStructureNode> nodes = [];
        IStructureNode root = MapElement("$root", element, nodes);

        return new StructureGraph
        {
            Root = root,
            Nodes = nodes
        };
    }

    // Recursively maps JSON values into structure graph nodes and records traversal order.
    private static IStructureNode MapElement(string name, JsonElement element, List<IStructureNode> nodes)
    {
        IStructureNode node = element.ValueKind switch
        {
            JsonValueKind.Object => MapObject(name, element, nodes),
            JsonValueKind.Array => MapArray(name, element, nodes),
            JsonValueKind.String => MapScalar(name, "String", element.GetString(), false),
            JsonValueKind.Number => MapScalar(name, "Number", element.GetRawText(), false),
            JsonValueKind.True => MapScalar(name, "Boolean", "true", false),
            JsonValueKind.False => MapScalar(name, "Boolean", "false", false),
            JsonValueKind.Null => MapScalar(name, "Null", string.Empty, true),
            _ => throw new InvalidOperationException("Unsupported JSON value kind.")
        };

        nodes.Add(node);
        return node;
    }

    // Maps JSON maps to Object nodes with property-name children.
    private static IStructureNode MapObject(string name, JsonElement element, List<IStructureNode> nodes)
    {
        List<IStructureNode> children = [];

        foreach (JsonProperty property in element.EnumerateObject())
        {
            children.Add(MapElement(property.Name, property.Value, nodes));
        }

        return new StructureNode
        {
            Name = name,
            Kind = StructureNodeKind.Object,
            Children = children
        };
    }

    // Maps JSON arrays to Array nodes with index-name children.
    private static IStructureNode MapArray(string name, JsonElement element, List<IStructureNode> nodes)
    {
        List<IStructureNode> children = [];
        int index = 0;

        foreach (JsonElement item in element.EnumerateArray())
        {
            children.Add(MapElement(index.ToString(CultureInfo.InvariantCulture), item, nodes));
            index++;
        }

        return new StructureNode
        {
            Name = name,
            Kind = StructureNodeKind.Array,
            Children = children
        };
    }

    // Maps JSON scalar tokens to scalar graph nodes.
    private static IStructureNode MapScalar(string name, string dataType, string rawValue, bool isNull)
    {
        return new ScalarStructureNode
        {
            Name = name,
            Value = new ScalarValue
            {
                DataType = dataType,
                RawValue = rawValue,
                IsNull = isNull
            }
        };
    }
}
