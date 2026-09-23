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
    /// Confirms that loaded documents preserve explicit validation assertions.
    /// </summary>
    [Fact]
    public void SessionLoadDocumentPreservesValidationAssertions()
    {
        IMappingDesignSession session = CreateSession();
        ValidationAssertion assertion = CreateValidationAssertion();
        TransformationDocument document = new()
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["source"] = CreateSchema("Source")
            },
            TargetSchema = CreateSchema("Target"),
            ValidationPayloadAlias = "source",
            ValidationSchemaKey = "source",
            ValidationAssertions = [assertion]
        };

        IMappingOperationResult result = session.LoadDocument(document);

        Assert.True(result.Succeeded);
        Assert.Equal("source", session.Document.ValidationPayloadAlias);
        Assert.Equal("source", session.Document.ValidationSchemaKey);
        Assert.Same(assertion, Assert.Single(session.Document.ValidationAssertions));
    }

    /// <summary>
    /// Confirms that sessions can replace explicit validation assertions.
    /// </summary>
    [Fact]
    public void SessionSetsValidationAssertions()
    {
        IMappingDesignSession session = CreateSession();

        IMappingOperationResult result = session.SetValidationAssertions("$source", "source", [CreateValidationAssertion()]);
        string dsl = session.ExportDsl();

        Assert.True(result.Succeeded);
        Assert.Equal("source", session.Document.ValidationPayloadAlias);
        Assert.Single(session.Document.ValidationAssertions);
        Assert.Contains("validate $source against source", dsl, System.StringComparison.Ordinal);
        Assert.Contains("assert exists($source.Customer.Name)", dsl, System.StringComparison.Ordinal);
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
    /// Confirms that imported DSL preserves explicit validation assertions.
    /// </summary>
    [Fact]
    public void SessionImportDslPreservesValidationAssertions()
    {
        IMappingDesignSession session = CreateSession();

        IMappingOperationResult result = session.ImportDsl(
            """
            validate $source against source {
              assert gt($source.Customer.Age, 10): "Customer must be adult."
            }
            """);
        string dsl = session.ExportDsl();

        Assert.True(result.Succeeded);
        Assert.Equal("source", session.Document.ValidationPayloadAlias);
        Assert.Equal("source", session.Document.ValidationSchemaKey);
        Assert.Single(session.Document.ValidationAssertions);
        Assert.Contains("assert gt($source.Customer.Age, 10): \"Customer must be adult.\"", dsl, System.StringComparison.Ordinal);
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

    // Creates a simple validation assertion for session tests.
    private static ValidationAssertion CreateValidationAssertion()
    {
        return new ValidationAssertion
        {
            Path = "$source.Customer.Name",
            Message = "Customer name is required.",
            Expression = new FunctionCallExpression
            {
                FunctionKey = "exists",
                Arguments =
                [
                    new PathExpression
                    {
                        Path = "$source.Customer.Name"
                    }
                ]
            }
        };
    }
}
