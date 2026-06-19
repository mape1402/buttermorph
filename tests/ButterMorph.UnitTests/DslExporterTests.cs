namespace ButterMorph.UnitTests;

using System.Collections.Generic;
using System.Linq;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Dsl;

/// <summary>
/// Verifies ButterMorph DSL export behavior.
/// </summary>
public sealed class DslExporterTests
{
    /// <summary>
    /// Confirms that metadata, nested target mappings and validations are exported.
    /// </summary>
    [Fact]
    public void ExportWritesMetadataTargetAndValidationBlocks()
    {
        ITransformationDocument document = new TransformationDocument
        {
            Metadata = new Dictionary<string, string>
            {
                ["version"] = "1",
                ["name"] = "CustomerMap"
            },
            Mappings =
            [
                CreateMapping(CreatePath("$source.Customer.Name"), "Customer.Name"),
                CreateMapping(CreatePath("$source.Customer.Email"), "Customer.Email")
            ],
            Validations =
            [
                CreateRule("Customer.Name", "required", [])
            ]
        };

        string dsl = new DslExporter().Export(document);

        Assert.Equal(
            """
            metadata {
              name: "CustomerMap"
              version: "1"
            }

            target {
              Customer {
                Name: $source.Customer.Name
                Email: $source.Customer.Email
              }
            }

            validate {
              Customer.Name: required
            }
            """,
            NormalizeLineEndings(dsl));
    }

    /// <summary>
    /// Confirms that supported expression shapes are exported.
    /// </summary>
    [Fact]
    public void ExportWritesSupportedExpressions()
    {
        ITransformationDocument document = new TransformationDocument
        {
            Mappings =
            [
                CreateMapping(CreateFunction(), "Customer.Display"),
                CreateMapping(CreateConditional(), "Customer.Status"),
                CreateMapping(CreateProjection(), "OrderIds"),
                CreateMapping(CreateMapExpression(), "Customer.Summary"),
                CreateMapping(CreateArrayExpression(), "Codes"),
                CreateMapping(CreateScalarCollection(), "Tags")
            ]
        };

        string dsl = new DslExporter().Export(document);

        Assert.Contains("Display: concat($source.Customer.Name, \" Lovelace\")", dsl, System.StringComparison.Ordinal);
        Assert.Contains("Status: when(true, \"Active\", \"Inactive\")", dsl, System.StringComparison.Ordinal);
        Assert.Contains("OrderIds: project $source.Orders as order => order.Id", dsl, System.StringComparison.Ordinal);
        Assert.Contains("Summary: { Name: $source.Customer.Name, Active: true }", dsl, System.StringComparison.Ordinal);
        Assert.Contains("Codes: [$source.Orders[0].Id, \"fallback\"]", dsl, System.StringComparison.Ordinal);
        Assert.Contains("Tags: scalars(\"a\", \"b\")", dsl, System.StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that strings are escaped and parse back correctly.
    /// </summary>
    [Fact]
    public void ExportEscapesStringsForRoundtrip()
    {
        ITransformationDocument document = new TransformationDocument
        {
            Mappings =
            [
                CreateMapping(CreateScalar("Line\nQuote\"Slash\\"), "Value")
            ]
        };

        string dsl = new DslExporter().Export(document);
        ITransformationDocument parsed = Parse(dsl);
        IScalarLiteralExpression scalar = Assert.IsAssignableFrom<IScalarLiteralExpression>(parsed.Mappings.Single().SourceExpression);

        Assert.Contains("Line\\nQuote\\\"Slash\\\\", dsl, System.StringComparison.Ordinal);
        Assert.Equal("Line\nQuote\"Slash\\", scalar.Value.RawValue);
    }

    /// <summary>
    /// Confirms that metadata keys with punctuation export and parse correctly.
    /// </summary>
    [Fact]
    public void ExportQuotesMetadataKeysThatAreNotIdentifiers()
    {
        ITransformationDocument document = new TransformationDocument
        {
            Metadata = new Dictionary<string, string>
            {
                ["playground-context"] = "complex"
            },
            Mappings =
            [
                CreateMapping(CreatePath("$source.Name"), "Name")
            ]
        };

        string dsl = new DslExporter().Export(document);
        ITransformationDocument parsed = Parse(dsl);

        Assert.Contains("\"playground-context\": \"complex\"", dsl, System.StringComparison.Ordinal);
        Assert.Equal("complex", parsed.Metadata["playground-context"]);
    }

    /// <summary>
    /// Confirms that exported DSL parses back into an equivalent document shape.
    /// </summary>
    [Fact]
    public void ExportRoundtripPreservesDocumentSemantics()
    {
        ITransformationDocument document = new TransformationDocument
        {
            Metadata = new Dictionary<string, string>
            {
                ["version"] = "1"
            },
            Mappings =
            [
                CreateMapping(CreateFunction(), "Customer.Display"),
                CreateMapping(CreateProjection(), "OrderIds"),
                CreateMapping(CreateConditional(), "Customer.Status"),
                CreateMapping(CreateMapExpression(), "Customer.Summary"),
                CreateMapping(CreateArrayExpression(), "Codes"),
                CreateMapping(CreateScalarCollection(), "Tags"),
                CreateMapping(CreatePath("$source.Orders[0].Id"), "Orders[0].Id")
            ],
            Validations =
            [
                CreateRule("Customer.Display", "required", []),
                CreateRule("Customer.Display", "min", [CreateNumber("2")])
            ]
        };

        string dsl = new DslExporter().Export(document);
        ITransformationDocument parsed = Parse(dsl);

        Assert.Equal(document.Metadata, parsed.Metadata);
        Assert.Equal(document.Mappings.Count, parsed.Mappings.Count);
        Assert.Equal(document.Validations.Count, parsed.Validations.Count);
        Assert.Contains(parsed.Mappings, mapping => string.Equals(mapping.TargetPath, "Orders[0].Id", System.StringComparison.Ordinal));
        Assert.IsAssignableFrom<IScalarCollectionLiteralExpression>(parsed.Mappings.ElementAt(5).SourceExpression);
        Assert.Equal("min", parsed.Validations.ElementAt(1).RuleKey);
        Assert.Single(parsed.Validations.ElementAt(1).Arguments);
    }

    /// <summary>
    /// Confirms that scalar collection syntax parses as a scalar collection literal.
    /// </summary>
    [Fact]
    public void ParseScalarsCreatesScalarCollectionLiteral()
    {
        ITransformationDocument document = Parse(
            """
            target {
              Tags: scalars("a", "b")
            }
            """);

        IScalarCollectionLiteralExpression expression = Assert.IsAssignableFrom<IScalarCollectionLiteralExpression>(document.Mappings.Single().SourceExpression);

        Assert.Equal(2, expression.Values.Count);
    }

    /// <summary>
    /// Confirms that scalar collection syntax rejects non-scalar arguments.
    /// </summary>
    [Fact]
    public void ParseScalarsRejectsNonScalarArguments()
    {
        Assert.Throws<FormatException>(() => Parse(
            """
            target {
              Tags: scalars($source.Tags)
            }
            """));
    }

    // Parses DSL text into a transformation document.
    private static ITransformationDocument Parse(string dsl)
    {
        IDslDocument document = new DslParser().Parse(new DslDefinition
        {
            Content = dsl
        });

        return Assert.IsAssignableFrom<ITransformationDocument>(document);
    }

    // Creates a transformation mapping.
    private static ITransformationMapping CreateMapping(ITransformationExpression expression, string targetPath)
    {
        return new TransformationMapping
        {
            SourceExpression = expression,
            TargetPath = targetPath
        };
    }

    // Creates a validation rule.
    private static IValidationRule CreateRule(string path, string ruleKey, IReadOnlyCollection<ITransformationExpression> arguments)
    {
        return new ValidationRule
        {
            Path = path,
            RuleKey = ruleKey,
            Arguments = arguments
        };
    }

    // Creates a path expression.
    private static IPathExpression CreatePath(string path)
    {
        return new PathExpression
        {
            Path = path
        };
    }

    // Creates a string scalar expression.
    private static IScalarLiteralExpression CreateScalar(string value)
    {
        return new ScalarLiteralExpression
        {
            Value = new ScalarValue
            {
                DataType = "String",
                RawValue = value,
                IsNull = false
            }
        };
    }

    // Creates a number scalar expression.
    private static IScalarLiteralExpression CreateNumber(string value)
    {
        return new ScalarLiteralExpression
        {
            Value = new ScalarValue
            {
                DataType = "Number",
                RawValue = value,
                IsNull = false
            }
        };
    }

    // Creates a boolean scalar expression.
    private static IScalarLiteralExpression CreateBoolean(bool value)
    {
        string rawValue = "false";

        if (value)
        {
            rawValue = "true";
        }

        return new ScalarLiteralExpression
        {
            Value = new ScalarValue
            {
                DataType = "Boolean",
                RawValue = rawValue,
                IsNull = false
            }
        };
    }

    // Creates a function call expression.
    private static IFunctionCallExpression CreateFunction()
    {
        return new FunctionCallExpression
        {
            FunctionKey = "concat",
            Arguments =
            [
                CreatePath("$source.Customer.Name"),
                CreateScalar(" Lovelace")
            ]
        };
    }

    // Creates a conditional expression.
    private static IConditionalExpression CreateConditional()
    {
        return new ConditionalExpression
        {
            Condition = CreateBoolean(true),
            ThenExpression = CreateScalar("Active"),
            ElseExpression = CreateScalar("Inactive")
        };
    }

    // Creates a collection projection expression.
    private static ICollectionProjectionExpression CreateProjection()
    {
        return new CollectionProjectionExpression
        {
            SourceExpression = CreatePath("$source.Orders"),
            ItemAlias = "order",
            BodyExpression = CreatePath("order.Id")
        };
    }

    // Creates an inline map-shaped expression.
    private static IObjectExpression CreateMapExpression()
    {
        return new ObjectExpression
        {
            Properties =
            [
                new ObjectPropertyExpression
                {
                    Name = "Name",
                    Expression = CreatePath("$source.Customer.Name")
                },
                new ObjectPropertyExpression
                {
                    Name = "Active",
                    Expression = CreateBoolean(true)
                }
            ]
        };
    }

    // Creates an ordered expression.
    private static IArrayExpression CreateArrayExpression()
    {
        return new ArrayExpression
        {
            Items =
            [
                CreatePath("$source.Orders[0].Id"),
                CreateScalar("fallback")
            ]
        };
    }

    // Creates a scalar collection literal expression.
    private static IScalarCollectionLiteralExpression CreateScalarCollection()
    {
        return new ScalarCollectionLiteralExpression
        {
            Values =
            [
                new ScalarValue
                {
                    DataType = "String",
                    RawValue = "a",
                    IsNull = false
                },
                new ScalarValue
                {
                    DataType = "String",
                    RawValue = "b",
                    IsNull = false
                }
            ]
        };
    }

    // Normalizes line endings for deterministic test assertions.
    private static string NormalizeLineEndings(string value)
    {
        return value.Replace("\r\n", "\n", System.StringComparison.Ordinal);
    }
}
