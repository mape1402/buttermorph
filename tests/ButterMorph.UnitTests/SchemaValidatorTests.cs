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
    /// Confirms that date, date-time, time, and duration schemas validate real payload values.
    /// </summary>
    [Fact]
    public void ValidateAcceptsTemporalPayloads()
    {
        SchemaValidator validator = new();
        ValidationRequest request = new()
        {
            SourceGraph = ReadJson(
                """
                {
                  "issuedOn": "2026-09-25",
                  "createdAt": "2026-09-25T14:30:00Z",
                  "cutoff": "14:30:00",
                  "duration": "PT2H30M"
                }
                """),
            Schema = CreateTemporalSchema()
        };

        ValidationResult result = validator.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    /// <summary>
    /// Confirms that temporal schema restrictions reject invalid and out-of-range payload values.
    /// </summary>
    [Fact]
    public void ValidateReportsTemporalRestrictionFailures()
    {
        SchemaValidator validator = new();
        ValidationRequest request = new()
        {
            SourceGraph = ReadJson(
                """
                {
                  "issuedOn": "2026-10-02",
                  "createdAt": "bad date",
                  "cutoff": "07:30:00",
                  "duration": "04:00:00"
                }
                """),
            Schema = CreateTemporalSchema()
        };

        ValidationResult result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "BMSV012" && diagnostic.Path == "issuedOn");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "BMSV002" && diagnostic.Path == "createdAt");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "BMSV011" && diagnostic.Path == "cutoff");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "BMSV012" && diagnostic.Path == "duration");
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

    private static IStructureSchema CreateTemporalSchema()
    {
        return new StructureSchema
        {
            Key = "Temporal",
            Name = "Temporal",
            Root = new SchemaNode
            {
                Name = "$root",
                Kind = SchemaNodeKind.Object,
                DataType = "object",
                Children =
                [
                    new SchemaNode
                    {
                        Name = "issuedOn",
                        Kind = SchemaNodeKind.Scalar,
                        DataType = "date",
                        IsRequired = true,
                        Metadata = new Dictionary<string, string>
                        {
                            ["minDate"] = "2026-09-01",
                            ["maxDate"] = "2026-09-30"
                        }
                    },
                    new SchemaNode
                    {
                        Name = "createdAt",
                        Kind = SchemaNodeKind.Scalar,
                        DataType = "datetime",
                        IsRequired = true,
                        Metadata = new Dictionary<string, string>
                        {
                            ["minDateTime"] = "2026-09-25T00:00:00Z",
                            ["maxDateTime"] = "2026-09-26T00:00:00Z"
                        }
                    },
                    new SchemaNode
                    {
                        Name = "cutoff",
                        Kind = SchemaNodeKind.Scalar,
                        DataType = "time",
                        IsRequired = true,
                        Metadata = new Dictionary<string, string>
                        {
                            ["minTime"] = "08:00:00",
                            ["maxTime"] = "18:00:00"
                        }
                    },
                    new SchemaNode
                    {
                        Name = "duration",
                        Kind = SchemaNodeKind.Scalar,
                        DataType = "timespan",
                        IsRequired = true,
                        Metadata = new Dictionary<string, string>
                        {
                            ["minDuration"] = "01:00:00",
                            ["maxDuration"] = "03:00:00"
                        }
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
