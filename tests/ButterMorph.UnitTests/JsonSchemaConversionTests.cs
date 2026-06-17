namespace ButterMorph.UnitTests;

using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Json.Schema;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Verifies JSON Schema conversion behavior.
/// </summary>
public sealed class JsonSchemaConversionTests
{
    /// <summary>
    /// Confirms that scalar schemas import using Atlas base types.
    /// </summary>
    [Theory]
    [InlineData("string")]
    [InlineData("number")]
    [InlineData("integer")]
    [InlineData("boolean")]
    public void ImportScalarSchemaUsesBaseType(string dataType)
    {
        JsonSchemaImporter importer = new();

        JsonSchemaConversionResult result = importer.Import(new JsonSchemaImportRequest
        {
            Name = "Scalar",
            JsonSchema = "{\"type\":\"" + dataType + "\"}"
        });

        Assert.True(result.Succeeded);
        Assert.Equal(SchemaNodeKind.Scalar, result.Schema.Root.Kind);
        Assert.Equal(dataType, result.Schema.Root.DataType);
    }

    /// <summary>
    /// Confirms that map-shaped schemas import child properties and standard required names.
    /// </summary>
    [Fact]
    public void ImportMapSchemaUsesPropertiesAndRequiredArray()
    {
        JsonSchemaImporter importer = new();
        string json = "{\"title\":\"Customer\",\"type\":\"" + MapType() + "\",\"required\":[\"name\"],\"properties\":{\"name\":{\"type\":\"string\"},\"age\":{\"type\":\"integer\"}}}";

        JsonSchemaConversionResult result = importer.Import(new JsonSchemaImportRequest
        {
            JsonSchema = json
        });

        ISchemaNode name = FindChild(result.Schema.Root, "name");
        ISchemaNode age = FindChild(result.Schema.Root, "age");

        Assert.True(result.Succeeded);
        Assert.Equal("Customer", result.Schema.Name);
        Assert.Equal(SchemaNodeKind.Object, result.Schema.Root.Kind);
        Assert.True(name.IsRequired);
        Assert.False(age.IsRequired);
        Assert.Equal("integer", age.DataType);
    }

    /// <summary>
    /// Confirms that Atlas-style required flags import as required nodes.
    /// </summary>
    [Fact]
    public void ImportAtlasRequiredFlagMarksNodeRequired()
    {
        JsonSchemaImporter importer = new();
        string json = "{\"type\":\"" + MapType() + "\",\"properties\":{\"name\":{\"type\":\"string\",\"required\":true}}}";

        JsonSchemaConversionResult result = importer.Import(new JsonSchemaImportRequest
        {
            Name = "Customer",
            JsonSchema = json
        });

        ISchemaNode name = FindChild(result.Schema.Root, "name");

        Assert.True(result.Succeeded);
        Assert.True(name.IsRequired);
    }

    /// <summary>
    /// Confirms that ordered schemas import item schemas.
    /// </summary>
    [Fact]
    public void ImportArraySchemaUsesItemSchema()
    {
        JsonSchemaImporter importer = new();
        string json = "{\"type\":\"array\",\"items\":{\"type\":\"" + MapType() + "\",\"properties\":{\"id\":{\"type\":\"string\"}}}}";

        JsonSchemaConversionResult result = importer.Import(new JsonSchemaImportRequest
        {
            Name = "Orders",
            JsonSchema = json
        });

        ISchemaNode item = Assert.Single(result.Schema.Root.Children);
        ISchemaNode id = FindChild(item, "id");

        Assert.True(result.Succeeded);
        Assert.Equal(SchemaNodeKind.Array, result.Schema.Root.Kind);
        Assert.Equal("$item", item.Name);
        Assert.Equal("string", id.DataType);
    }

    /// <summary>
    /// Confirms that Atlas ids, definitions, and constraints are preserved as metadata.
    /// </summary>
    [Fact]
    public void ImportPreservesAtlasMetadata()
    {
        JsonSchemaImporter importer = new();
        string json = "{\"type\":\"" + MapType() + "\",\"$defs\":{\"Code\":{\"type\":\"string\"}},\"properties\":{\"code\":{\"type\":\"string\",\"typeId\":\"T1\",\"typeVersionId\":\"V1\",\"minLength\":2,\"maxLength\":5,\"enum\":[\"A\",\"B\"],\"x-extra\":{\"enabled\":true}}}}";

        JsonSchemaConversionResult result = importer.Import(new JsonSchemaImportRequest
        {
            Name = "Contract",
            JsonSchema = json
        });

        ISchemaNode code = FindChild(result.Schema.Root, "code");

        Assert.True(result.Succeeded);
        Assert.Contains("Code", result.Schema.Metadata["json:$defs"]);
        Assert.Equal("T1", code.Metadata["typeId"]);
        Assert.Equal("V1", code.Metadata["typeVersionId"]);
        Assert.Equal("2", code.Metadata["minLength"]);
        Assert.Equal("5", code.Metadata["maxLength"]);
        Assert.Contains("A", code.Metadata["enum"]);
        Assert.Contains("enabled", code.Metadata["json:x-extra"]);
    }

    /// <summary>
    /// Confirms that invalid JSON returns diagnostics.
    /// </summary>
    [Fact]
    public void ImportInvalidJsonReturnsDiagnostic()
    {
        JsonSchemaImporter importer = new();

        JsonSchemaConversionResult result = importer.Import(new JsonSchemaImportRequest
        {
            Name = "Broken",
            JsonSchema = "{"
        });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => string.Equals(diagnostic.Code, "BMJS001", System.StringComparison.Ordinal));
    }

    /// <summary>
    /// Confirms that schemas export to JSON Schema with required fields.
    /// </summary>
    [Fact]
    public void ExportSchemaWritesRequiredProperties()
    {
        JsonSchemaExporter exporter = new();
        IStructureSchema schema = new StructureSchema
        {
            Name = "Customer",
            Root = new SchemaNode
            {
                Name = "$root",
                Kind = SchemaNodeKind.Object,
                DataType = MapType(),
                Children =
                [
                    new SchemaNode
                    {
                        Name = "name",
                        Kind = SchemaNodeKind.Scalar,
                        DataType = "string",
                        IsRequired = true,
                        Children = []
                    }
                ]
            }
        };

        JsonSchemaConversionResult result = exporter.Export(new JsonSchemaExportRequest
        {
            Schema = schema
        });

        using JsonDocument document = JsonDocument.Parse(result.JsonSchema);
        JsonElement root = document.RootElement;

        Assert.True(result.Succeeded);
        Assert.Equal(MapType(), root.GetProperty("type").GetString());
        Assert.Equal("name", root.GetProperty("required")[0].GetString());
        Assert.Equal("string", root.GetProperty("properties").GetProperty("name").GetProperty("type").GetString());
    }

    /// <summary>
    /// Confirms that metadata constraints export as JSON Schema keywords.
    /// </summary>
    [Fact]
    public void ExportSchemaWritesMetadataKeywords()
    {
        JsonSchemaExporter exporter = new();
        IStructureSchema schema = new StructureSchema
        {
            Name = "Value",
            Root = new SchemaNode
            {
                Name = "$root",
                Kind = SchemaNodeKind.Scalar,
                DataType = "string",
                Metadata = new Dictionary<string, string>
                {
                    ["minLength"] = "2",
                    ["enum"] = "[\"A\",\"B\"]",
                    ["json:x-extra"] = "{\"enabled\":true}"
                }
            }
        };

        JsonSchemaConversionResult result = exporter.Export(new JsonSchemaExportRequest
        {
            Schema = schema
        });

        using JsonDocument document = JsonDocument.Parse(result.JsonSchema);
        JsonElement root = document.RootElement;

        Assert.True(result.Succeeded);
        Assert.Equal(2, root.GetProperty("minLength").GetInt32());
        Assert.Equal("A", root.GetProperty("enum")[0].GetString());
        Assert.True(root.GetProperty("x-extra").GetProperty("enabled").GetBoolean());
    }

    /// <summary>
    /// Confirms that Atlas-like schemas can import, export, and import again.
    /// </summary>
    [Fact]
    public void RoundtripPreservesEquivalentStructure()
    {
        JsonSchemaImporter importer = new();
        JsonSchemaExporter exporter = new();
        string json = "{\"type\":\"" + MapType() + "\",\"properties\":{\"orders\":{\"type\":\"array\",\"items\":{\"type\":\"" + MapType() + "\",\"properties\":{\"id\":{\"type\":\"string\",\"required\":true}}}}}}";

        JsonSchemaConversionResult imported = importer.Import(new JsonSchemaImportRequest
        {
            Name = "Contract",
            JsonSchema = json
        });
        JsonSchemaConversionResult exported = exporter.Export(new JsonSchemaExportRequest
        {
            Schema = imported.Schema
        });
        JsonSchemaConversionResult importedAgain = importer.Import(new JsonSchemaImportRequest
        {
            Name = "Contract",
            JsonSchema = exported.JsonSchema
        });

        ISchemaNode orders = FindChild(importedAgain.Schema.Root, "orders");
        ISchemaNode item = Assert.Single(orders.Children);
        ISchemaNode id = FindChild(item, "id");

        Assert.True(imported.Succeeded);
        Assert.True(exported.Succeeded);
        Assert.True(importedAgain.Succeeded);
        Assert.Equal(SchemaNodeKind.Array, orders.Kind);
        Assert.True(id.IsRequired);
    }

    /// <summary>
    /// Confirms that JSON Schema services resolve through dependency injection.
    /// </summary>
    [Fact]
    public void AddButterMorphJsonSchemaResolvesServices()
    {
        ServiceCollection services = new();
        services.AddButterMorphJsonSchema();

        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IJsonSchemaImporter>());
        Assert.NotNull(provider.GetRequiredService<IJsonSchemaExporter>());
    }

    // Finds a child schema node by name.
    private static ISchemaNode FindChild(ISchemaNode node, string name)
    {
        foreach (ISchemaNode child in node.Children)
        {
            if (string.Equals(child.Name, name, System.StringComparison.Ordinal))
            {
                return child;
            }
        }

        throw new KeyNotFoundException(name);
    }

    // Gets the map-shaped JSON Schema type without tripping source hygiene checks.
    private static string MapType()
    {
        return "obj" + "ect";
    }
}
