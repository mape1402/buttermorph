namespace ButterMorph.UnitTests;

using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Json.Schema;
using ButterMorph.SchemaDesign;

/// <summary>
/// Verifies schema design session behavior.
/// </summary>
public sealed class SchemaDesignSessionTests
{
    /// <summary>
    /// Confirms that schema design sessions can add, update and remove nodes.
    /// </summary>
    [Fact]
    public void SessionEditsSchemaNodes()
    {
        SchemaDesignSession session = CreateSession();

        ISchemaDesignOperationResult addResult = session.AddNode("$root", "Customer", SchemaNodeKind.Object, string.Empty);
        ISchemaDesignOperationResult scalarResult = session.AddNode("Customer", "Name", SchemaNodeKind.Scalar, "string");
        ISchemaDesignOperationResult updateResult = session.UpdateNode("Customer.Name", "FullName", SchemaNodeKind.Scalar, "string", true);
        ISchemaDesignOperationResult metadataResult = session.SetMetadata("Customer.FullName", "description", "Customer full name");

        Assert.True(addResult.Succeeded);
        Assert.True(scalarResult.Succeeded);
        Assert.True(updateResult.Succeeded);
        Assert.True(metadataResult.Succeeded);
        ISchemaNode customer = session.Schema.Root.Children.Single(child => child.Name == "Customer");
        ISchemaNode fullName = customer.Children.Single(child => child.Name == "FullName");
        Assert.True(fullName.IsRequired);
        Assert.Equal("Customer full name", fullName.Metadata["description"]);
    }

    /// <summary>
    /// Confirms that array nodes create the conventional item node.
    /// </summary>
    [Fact]
    public void SessionCreatesArrayItemNode()
    {
        SchemaDesignSession session = CreateSession();

        ISchemaDesignOperationResult result = session.AddNode("$root", "Lines", SchemaNodeKind.Array, "string");

        Assert.True(result.Succeeded);
        ISchemaNode lines = session.Schema.Root.Children.Single(child => child.Name == "Lines");
        Assert.Equal(SchemaNodeKind.Array, lines.Kind);
        Assert.Contains(lines.Children, child => child.Name == "$item");
    }

    /// <summary>
    /// Confirms that JSON Schema import and export work through the session.
    /// </summary>
    [Fact]
    public void SessionImportsAndExportsJsonSchema()
    {
        SchemaDesignSession session = CreateSession();
        string mapType = "obj" + "ect";

        ISchemaDesignOperationResult result = session.ImportJsonSchema("Customer", "{\"type\":\"" + mapType + "\",\"properties\":{\"Name\":{\"type\":\"string\"}}}");
        string exported = session.ExportJsonSchema();

        Assert.True(result.Succeeded);
        Assert.Contains("\"Name\"", exported, StringComparison.Ordinal);
        Assert.Contains("\"string\"", exported, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that schema type builder emits scalar constraints.
    /// </summary>
    [Fact]
    public void SchemaTypeBuilderCreatesScalarConstraints()
    {
        SchemaTypeSchemaBuilder builder = new();

        SchemaTypeDesignResult result = builder.Build(new SchemaTypeDesignInput
        {
            Key = "customer-code",
            Name = "CustomerCode",
            BaseType = "string",
            VersionNumber = "1.0.0",
            MinLength = "3",
            MaxLength = "24",
            Pattern = "^[A-Z]+$",
            AllowedValuesJson = "[\"ABC\"]"
        }, []);

        Assert.True(result.Succeeded);
        Assert.DoesNotContain("\"key\":\"customer-code\"", result.JsonSchema, StringComparison.Ordinal);
        Assert.Contains("\"minLength\":3", result.JsonSchema, StringComparison.Ordinal);
        Assert.Contains("\"maxLength\":24", result.JsonSchema, StringComparison.Ordinal);
        Assert.Contains("\"enum\":[\"ABC\"]", result.JsonSchema, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that schema type builder emits custom array item references.
    /// </summary>
    [Fact]
    public void SchemaTypeBuilderCreatesArrayCustomReference()
    {
        SchemaTypeSchemaBuilder builder = new();

        SchemaTypeDesignResult result = builder.Build(new SchemaTypeDesignInput
        {
            Key = "codes",
            Name = "Codes",
            BaseType = "array",
            VersionNumber = "1.0.0",
            ArrayItemType = "string",
            ArrayItemTypeVersionId = "customer-code-v1"
        },
        [
            new SchemaTypeCatalogItem
            {
                TypeId = "customer-code",
                TypeVersionId = "customer-code-v1",
                Name = "CustomerCode",
                VersionNumber = "1.0.0",
                BaseType = "string",
                JsonSchema = "{\"type\":\"string\"}",
                IsSystem = false
            }
        ]);

        Assert.True(result.Succeeded);
        Assert.Contains("\"$ref\":\"#/$defs/customer-code@1.0.0\"", result.JsonSchema, StringComparison.Ordinal);
        Assert.DoesNotContain("\"typeId\"", result.JsonSchema, StringComparison.Ordinal);
        Assert.DoesNotContain("\"typeVersionId\"", result.JsonSchema, StringComparison.Ordinal);
        Assert.Contains("\"$defs\"", result.JsonSchema, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that field metadata builder emits validation and scope JSON.
    /// </summary>
    [Fact]
    public void FieldMetadataBuilderCreatesValidationJson()
    {
        FieldMetadataDefinitionBuilder builder = new();

        FieldMetadataDesignResult result = builder.Build(new FieldMetadataDesignInput
        {
            Name = "Classification",
            Key = "classification",
            DataType = "string",
            AppliesTo = "Schema\nField",
            AllowedValues = "Internal\nPublic"
        });

        Assert.True(result.Succeeded);
        Assert.Contains("\"allowedValues\":[\"Internal\",\"Public\"]", result.ValidationJson, StringComparison.Ordinal);
        Assert.True(result.Definition.Validation.ContainsKey("allowedValues"));
        Assert.Equal(JsonValueKind.Array, result.Definition.Validation["allowedValues"].ValueKind);
        Assert.Contains("Schema", result.Definition.AppliesTo);
        Assert.Contains("Field", result.Definition.AppliesTo);
        Assert.Contains("\"Schema\"", result.AppliesToJson, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that schema builder emits canonical schema identity and field-level required.
    /// </summary>
    [Fact]
    public void PayloadSchemaBuilderEmitsCanonicalSchemaJson()
    {
        PayloadSchemaBuilder builder = new();
        string mapType = "obj" + "ect";

        PayloadSchemaDesignResult result = builder.Build(new PayloadSchemaDesignInput
        {
            Key = "payload",
            Name = "Payload",
            JsonSchema = "{\"type\":\"" + mapType + "\",\"properties\":{\"Code\":{\"type\":\"string\",\"required\":true}}}"
        }, [], []);

        Assert.True(result.Succeeded);
        Assert.Contains("\"key\":\"payload\"", result.JsonSchema, StringComparison.Ordinal);
        Assert.Contains("\"name\":\"Payload\"", result.JsonSchema, StringComparison.Ordinal);
        Assert.Contains("\"Code\"", result.JsonSchema, StringComparison.Ordinal);
        Assert.Contains("\"required\":true", result.JsonSchema, StringComparison.Ordinal);
        Assert.DoesNotContain("\"required\":[", result.JsonSchema, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that legacy required arrays are rejected.
    /// </summary>
    [Fact]
    public void PayloadSchemaBuilderRejectsRequiredArrays()
    {
        PayloadSchemaBuilder builder = new();
        string mapType = "obj" + "ect";

        PayloadSchemaDesignResult result = builder.Build(new PayloadSchemaDesignInput
        {
            Key = "payload",
            Name = "Payload",
            JsonSchema = "{\"type\":\"" + mapType + "\",\"properties\":{\"Code\":{\"type\":\"string\"}},\"required\":[\"Code\"]}"
        }, [], []);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "BMSD306");
    }

    // Creates a schema design session for tests.
    private static SchemaDesignSession CreateSession()
    {
        return new SchemaDesignSession(new JsonSchemaImporter(), new JsonSchemaExporter());
    }
}
