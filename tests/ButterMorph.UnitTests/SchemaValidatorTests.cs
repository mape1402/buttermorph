namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Json;
using ButterMorph.Validation;

/// <summary>
/// Verifies schema validation against real payload graphs.
/// </summary>
public sealed class SchemaValidatorTests
{
    /// <summary>
    /// Confirms that valid payloads pass required, type and constraint checks.
    /// </summary>
    [Fact]
    public void ValidateReturnsValidWhenPayloadMatchesSchema()
    {
        SchemaValidator validator = new();
        ValidationRequest request = new()
        {
            SourceGraph = ReadJson(
                """
                {
                  "name": "Ada",
                  "quantity": 12,
                  "tags": ["vip"]
                }
                """),
            Schema = CreateOrderSchema()
        };

        ValidationResult result = validator.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    /// <summary>
    /// Confirms that required fields and schema metadata restrictions are enforced.
    /// </summary>
    [Fact]
    public void ValidateReportsRequiredTypeAndRestrictionFailures()
    {
        SchemaValidator validator = new();
        ValidationRequest request = new()
        {
            SourceGraph = ReadJson(
                """
                {
                  "name": "Al",
                  "quantity": 2.5,
                  "tags": ["a", "b", "c"]
                }
                """),
            Schema = CreateOrderSchema()
        };

        ValidationResult result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "BMSV003" && diagnostic.Path == "name");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "BMSV002" && diagnostic.Path == "quantity");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "BMSV008" && diagnostic.Path == "tags");
    }

    /// <summary>
    /// Confirms that missing required payload fields are reported.
    /// </summary>
    [Fact]
    public void ValidateReportsMissingRequiredField()
    {
        SchemaValidator validator = new();
        ValidationRequest request = new()
        {
            SourceGraph = ReadJson("{\"quantity\":12}"),
            Schema = CreateOrderSchema()
        };

        ValidationResult result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "BMSV001" && diagnostic.Path == "name");
    }

    /// <summary>
    /// Confirms that schema references resolve custom type restrictions from definitions.
    /// </summary>
    [Fact]
    public void ValidateResolvesDefinitionReferences()
    {
        SchemaValidator validator = new();
        ValidationRequest request = new()
        {
            SourceGraph = ReadJson("{\"id\":\"ABC\"}"),
            Schema = new StructureSchema
            {
                Key = "Contract",
                Metadata = new Dictionary<string, string>
                {
                    ["json:$defs"] = "{\"CustomId\":{\"type\":\"string\",\"minLength\":5}}"
                },
                Root = new SchemaNode
                {
                    Name = "$root",
                    Kind = SchemaNodeKind.Object,
                    DataType = "object",
                    Children =
                    [
                        new SchemaNode
                        {
                            Name = "id",
                            Kind = SchemaNodeKind.Object,
                            DataType = "object",
                            Metadata = new Dictionary<string, string>
                            {
                                ["$ref"] = "#/$defs/CustomId"
                            }
                        }
                    ]
                }
            }
        };

        ValidationResult result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "BMSV003" && diagnostic.Path == "id");
    }

    private static IStructureSchema CreateOrderSchema()
    {
        return new StructureSchema
        {
            Key = "Order",
            Name = "Order",
            Root = new SchemaNode
            {
                Name = "$root",
                Kind = SchemaNodeKind.Object,
                DataType = "object",
                Children =
                [
                    new SchemaNode
                    {
                        Name = "name",
                        Kind = SchemaNodeKind.Scalar,
                        DataType = "string",
                        IsRequired = true,
                        Metadata = new Dictionary<string, string>
                        {
                            ["minLength"] = "3",
                            ["maxLength"] = "10"
                        }
                    },
                    new SchemaNode
                    {
                        Name = "quantity",
                        Kind = SchemaNodeKind.Scalar,
                        DataType = "integer",
                        IsRequired = true,
                        Metadata = new Dictionary<string, string>
                        {
                            ["minimum"] = "1",
                            ["maximum"] = "20"
                        }
                    },
                    new SchemaNode
                    {
                        Name = "tags",
                        Kind = SchemaNodeKind.Array,
                        DataType = "array",
                        Metadata = new Dictionary<string, string>
                        {
                            ["minItems"] = "1",
                            ["maxItems"] = "2"
                        },
                        Children =
                        [
                            new SchemaNode
                            {
                                Name = "$item",
                                Kind = SchemaNodeKind.Scalar,
                                DataType = "string"
                            }
                        ]
                    }
                ]
            }
        };
    }

    private static IStructureGraph ReadJson(string json)
    {
        return new JsonReader().Read(new StructureInput
        {
            Format = "json",
            Content = json
        });
    }
}
