namespace ButterMorph.Functions;

using System.Globalization;
using System.Text;
using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Core;

internal sealed class JsonFunctionConverter
{
    internal IStructureNode ReadNode(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return ReadElement("$json", document.RootElement);
    }

    internal string WriteNode(IStructureNode node)
    {
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);
        WriteNode(writer, node);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    internal string WriteScalar(IScalarValue value)
    {
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);
        WriteScalar(writer, value);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Reads one JSON element into a structure node.
    private IStructureNode ReadElement(string name, JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            List<IStructureNode> children = [];

            foreach (JsonProperty property in element.EnumerateObject())
            {
                children.Add(ReadElement(property.Name, property.Value));
            }

            return new StructureNode
            {
                Name = name,
                Kind = StructureNodeKind.Object,
                Children = children
            };
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            List<IStructureNode> children = [];
            int index = 0;

            foreach (JsonElement item in element.EnumerateArray())
            {
                children.Add(ReadElement(index.ToString(CultureInfo.InvariantCulture), item));
                index++;
            }

            return new StructureNode
            {
                Name = name,
                Kind = StructureNodeKind.Array,
                Children = children
            };
        }

        return new ScalarStructureNode
        {
            Name = name,
            Value = ReadScalar(element)
        };
    }

    // Reads one JSON scalar into a ButterMorph scalar value.
    private IScalarValue ReadScalar(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            string text = element.GetString();

            return new ScalarValue
            {
                DataType = "String",
                RawValue = text,
                IsNull = false
            };
        }

        if (element.ValueKind == JsonValueKind.Number)
        {
            return new ScalarValue
            {
                DataType = "Number",
                RawValue = element.GetRawText(),
                IsNull = false
            };
        }

        if (element.ValueKind == JsonValueKind.True)
        {
            return new ScalarValue
            {
                DataType = "Boolean",
                RawValue = "true",
                IsNull = false
            };
        }

        if (element.ValueKind == JsonValueKind.False)
        {
            return new ScalarValue
            {
                DataType = "Boolean",
                RawValue = "false",
                IsNull = false
            };
        }

        return new ScalarValue
        {
            DataType = "Null",
            RawValue = string.Empty,
            IsNull = true
        };
    }

    // Writes one structure node as JSON.
    private void WriteNode(Utf8JsonWriter writer, IStructureNode node)
    {
        if (node is IScalarStructureNode scalarNode)
        {
            WriteScalar(writer, scalarNode.Value);
            return;
        }

        if (node.Kind == StructureNodeKind.Array)
        {
            writer.WriteStartArray();

            foreach (IStructureNode child in node.Children)
            {
                WriteNode(writer, child);
            }

            writer.WriteEndArray();
            return;
        }

        writer.WriteStartObject();

        foreach (IStructureNode child in node.Children)
        {
            writer.WritePropertyName(child.Name);
            WriteNode(writer, child);
        }

        writer.WriteEndObject();
    }

    // Writes one scalar value as JSON.
    private void WriteScalar(Utf8JsonWriter writer, IScalarValue value)
    {
        if (value.IsNull)
        {
            writer.WriteNullValue();
            return;
        }

        if (string.Equals(value.DataType, "Number", StringComparison.OrdinalIgnoreCase))
        {
            if (decimal.TryParse(value.RawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
            {
                writer.WriteNumberValue(decimalValue);
                return;
            }
        }

        if (string.Equals(value.DataType, "Boolean", StringComparison.OrdinalIgnoreCase))
        {
            if (bool.TryParse(value.RawValue, out bool booleanValue))
            {
                writer.WriteBooleanValue(booleanValue);
                return;
            }
        }

        writer.WriteStringValue(value.RawValue);
    }
}
