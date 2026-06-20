namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Design;
using ButterMorph.Dsl;

/// <summary>
/// Verifies mapping design session behavior.
/// </summary>
public sealed class DesignSessionTests
{
    /// <summary>
    /// Confirms that sessions can load schemas and edit mappings.
    /// </summary>
    [Fact]
    public void SessionLoadsSchemasAndEditsMappings()
    {
        IMappingDesignSession session = CreateSession();

        IMappingOperationResult sourceResult = session.LoadSourceSchema("source", CreateSchema("Source"));
        IMappingOperationResult targetResult = session.LoadTargetSchema(CreateSchema("Target"));
        IMappingOperationResult addResult = session.AddPathMapping("$source.Customer.Name", "Customer.Name");
        IMappingOperationResult removeResult = session.RemoveMapping("Customer.Name");

        Assert.True(sourceResult.Succeeded);
        Assert.True(targetResult.Succeeded);
        Assert.True(addResult.Succeeded);
        Assert.True(removeResult.Succeeded);
        Assert.True(session.Document.SourceSchemas.ContainsKey("source"));
        Assert.Empty(session.Document.Mappings);
    }

    /// <summary>
    /// Confirms that sessions can edit validation rules.
    /// </summary>
    [Fact]
    public void SessionAddsAndRemovesValidationRules()
    {
        IMappingDesignSession session = CreateSession();
        ValidationRule rule = new()
        {
            Path = "Customer.Name",
            RuleKey = "required",
            Arguments = []
        };

        session.AddValidationRule(rule);
        session.RemoveValidationRule("Customer.Name", "required");

        Assert.Empty(session.Document.Validations);
    }

    /// <summary>
    /// Confirms that invalid user operations return diagnostics.
    /// </summary>
    [Fact]
    public void SessionReturnsDiagnosticsForInvalidOperations()
    {
        IMappingDesignSession session = CreateSession();

        IMappingOperationResult result = session.AddPathMapping(string.Empty, "Customer.Name");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "BMDG002");
    }

    /// <summary>
    /// Confirms that sessions can add mapping expressions from DSL expression text.
    /// </summary>
    [Fact]
    public void SessionAddsExpressionTextMappings()
    {
        IMappingDesignSession session = CreateSession();

        IMappingOperationResult result = session.AddExpressionTextMapping("concat($source.Customer.Name, \"!\")", "Customer.Name");

        Assert.True(result.Succeeded);
        Assert.Single(session.Document.Mappings);
        Assert.IsAssignableFrom<IFunctionCallExpression>(session.Document.Mappings.First().SourceExpression);
    }

    /// <summary>
    /// Confirms that sessions import and export DSL.
    /// </summary>
    [Fact]
    public void SessionImportsAndExportsDsl()
    {
        IMappingDesignSession session = CreateSession();

        IMappingOperationResult result = session.ImportDsl(
            """
            metadata {
              version: "1"
            }

            target {
              Customer {
                Name: $source.Customer.Name
              }
            }
            """);
        string dsl = session.ExportDsl();

        Assert.True(result.Succeeded);
        Assert.Contains("Customer", dsl, System.StringComparison.Ordinal);
        Assert.Contains("version", dsl, System.StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that sessions execute semantic analysis.
    /// </summary>
    [Fact]
    public void SessionRunsSemanticAnalysis()
    {
        IMappingDesignSession session = CreateSession();

        SemanticAnalysisResult result = session.Analyze();

        Assert.True(result.Succeeded);
    }

    // Creates a test session.
    private static IMappingDesignSession CreateSession()
    {
        return new MappingDesignSession(new DslParser(), new DslExporter(), new PassingSemanticAnalyzer());
    }

    // Creates a simple schema for tests.
    private static IStructureSchema CreateSchema(string name)
    {
        return new StructureSchema
        {
            Key = name.ToLowerInvariant(),
            Name = name,
            Root = new SchemaNode
            {
                Name = "$root",
                Kind = SchemaNodeKind.Object,
                Children =
                [
                    new SchemaNode
                    {
                        Name = "Customer",
                        Kind = SchemaNodeKind.Object,
                        Children =
                        [
                            new SchemaNode
                            {
                                Name = "Name",
                                Kind = SchemaNodeKind.Scalar,
                                DataType = "String"
                            }
                        ]
                    }
                ]
            }
        };
    }
}
