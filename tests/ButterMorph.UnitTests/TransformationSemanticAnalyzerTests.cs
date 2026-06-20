namespace ButterMorph.UnitTests;

using System.Collections.Generic;
using System.Linq;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Functions;
using ButterMorph.Semantics;

/// <summary>
/// Verifies transformation semantic analysis behavior.
/// </summary>
public sealed class TransformationSemanticAnalyzerTests
{
    /// <summary>
    /// Confirms that a valid document produces no diagnostics.
    /// </summary>
    [Fact]
    public void AnalyzeReturnsSuccessForValidDocument()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [
            CreateMapping(CreatePath("$source.Customer.Name"), "Customer.Name"),
            CreateMapping(CreateConcatExpression(), "Customer.Display"),
            CreateMapping(CreateProjectionExpression(), "OrderIds")
        ],
        [
            CreateRule("Customer.Name", "required", []),
            CreateRule("Customer.Name", "min", [CreateNumber("2")])
        ]);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Diagnostics);
    }

    /// <summary>
    /// Confirms that missing source schemas are reported.
    /// </summary>
    [Fact]
    public void AnalyzeReportsMissingSourceSchema()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = new TransformationDocument
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>(),
            TargetSchema = CreateTargetSchema(),
            Mappings =
            [
                CreateMapping(CreatePath("$source.Customer.Name"), "Customer.Name")
            ]
        };

        SemanticAnalysisResult result = analyzer.Analyze(document);

        AssertDiagnostic(result, "BMSM001");
    }

    /// <summary>
    /// Confirms that invalid source paths are reported.
    /// </summary>
    [Fact]
    public void AnalyzeReportsInvalidSourcePath()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [
            CreateMapping(CreatePath("$source.Customer.Unknown"), "Customer.Name")
        ],
        []);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        AssertDiagnostic(result, "BMSM002");
    }

    /// <summary>
    /// Confirms that invalid target paths are reported.
    /// </summary>
    [Fact]
    public void AnalyzeReportsInvalidTargetPath()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [
            CreateMapping(CreatePath("$source.Customer.Name"), "Customer.Unknown")
        ],
        []);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        AssertDiagnostic(result, "BMSM003");
    }

    /// <summary>
    /// Confirms that scalar data type differences do not block mappings.
    /// </summary>
    [Fact]
    public void AnalyzeAllowsDifferentScalarDataTypes()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = new TransformationDocument
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["source"] = CreateSourceSchema()
            },
            TargetSchema = CreateNumberTargetSchema(),
            Mappings =
            [
                CreateMapping(CreatePath("$source.Customer.Name"), "Customer.Amount")
            ],
            Validations = []
        };

        SemanticAnalysisResult result = analyzer.Analyze(document);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Diagnostics);
    }

    /// <summary>
    /// Confirms that missing function descriptors are reported.
    /// </summary>
    [Fact]
    public void AnalyzeReportsMissingFunctionDescriptor()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(new FunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [
            CreateMapping(CreateConcatExpression(), "Customer.Display")
        ],
        []);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        AssertDiagnostic(result, "BMSM004");
    }

    /// <summary>
    /// Confirms that invalid function argument count is reported.
    /// </summary>
    [Fact]
    public void AnalyzeReportsFunctionArgumentCountMismatch()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [
            CreateMapping(new FunctionCallExpression
            {
                FunctionKey = "concat",
                Arguments = []
            }, "Customer.Display")
        ],
        []);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        AssertDiagnostic(result, "BMSM005");
    }

    /// <summary>
    /// Confirms that invalid function argument kind is reported.
    /// </summary>
    [Fact]
    public void AnalyzeReportsFunctionArgumentKindMismatch()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [
            CreateMapping(new FunctionCallExpression
            {
                FunctionKey = "nodeOnly",
                Arguments =
                [
                    CreatePath("$source.Customer.Name")
                ]
            }, "Customer.Display")
        ],
        []);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        AssertDiagnostic(result, "BMSM006");
    }

    /// <summary>
    /// Confirms that newly added native-style function descriptors are accepted.
    /// </summary>
    [Fact]
    public void AnalyzeAcceptsExpandedFunctionDescriptors()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [
            CreateMapping(CreateFunction("split", [CreatePath("$source.Customer.Name"), CreateScalar(",")]), "OrderIds"),
            CreateMapping(CreateFunction("toNumber", [CreatePath("$source.Customer.Name")]), "Customer.Display"),
            CreateMapping(CreateFunction("camelCase", [CreatePath("$source.Customer.Name")]), "Customer.Display"),
            CreateMapping(CreateFunction("ToUpper", [CreatePath("$source.Customer.Name")]), "Customer.Display"),
            CreateMapping(CreateFunction("sum", [CreateFunction("split", [CreatePath("$source.Customer.Name"), CreateScalar(",")])]), "Customer.Display")
        ],
        []);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Diagnostics);
    }

    /// <summary>
    /// Confirms that missing validation rule descriptors are reported.
    /// </summary>
    [Fact]
    public void AnalyzeReportsMissingValidationRuleDescriptor()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), new ValidationRuleRegistry());
        ITransformationDocument document = CreateDocument(
        [],
        [
            CreateRule("Customer.Name", "required", [])
        ]);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        AssertDiagnostic(result, "BMSM007");
    }

    /// <summary>
    /// Confirms that invalid validation argument count is reported.
    /// </summary>
    [Fact]
    public void AnalyzeReportsValidationArgumentCountMismatch()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [],
        [
            CreateRule("Customer.Name", "min", [])
        ]);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        AssertDiagnostic(result, "BMSM008");
    }

    /// <summary>
    /// Confirms that invalid validation argument kind is reported.
    /// </summary>
    [Fact]
    public void AnalyzeReportsValidationArgumentKindMismatch()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [],
        [
            CreateRule("Customer.Name", "nodeRule", [CreateNumber("2")])
        ]);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        AssertDiagnostic(result, "BMSM009");
    }

    /// <summary>
    /// Confirms that projection source shape is validated.
    /// </summary>
    [Fact]
    public void AnalyzeReportsInvalidProjectionSource()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [
            CreateMapping(new CollectionProjectionExpression
            {
                SourceExpression = CreatePath("$source.Customer.Name"),
                ItemAlias = "item",
                BodyExpression = CreatePath("item.Id")
            }, "OrderIds")
        ],
        []);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        AssertDiagnostic(result, "BMSM010");
    }

    /// <summary>
    /// Confirms that projection alias scope is accepted.
    /// </summary>
    [Fact]
    public void AnalyzeAcceptsProjectionAliasScope()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [
            CreateMapping(CreateProjectionExpression(), "OrderIds")
        ],
        []);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        Assert.True(result.Succeeded);
    }

    /// <summary>
    /// Confirms that invalid aliases are reported.
    /// </summary>
    [Fact]
    public void AnalyzeReportsInvalidAlias()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [
            CreateMapping(CreatePath("missing.Id"), "Customer.Display")
        ],
        []);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        AssertDiagnostic(result, "BMSM011");
    }

    /// <summary>
    /// Confirms that incompatible conditional branches are reported.
    /// </summary>
    [Fact]
    public void AnalyzeReportsConditionalBranchMismatch()
    {
        TransformationSemanticAnalyzer analyzer = CreateAnalyzer(CreateFunctionRegistry(), CreateValidationRegistry());
        ITransformationDocument document = CreateDocument(
        [
            CreateMapping(new ConditionalExpression
            {
                Condition = CreateBoolean(true),
                ThenExpression = CreatePath("$source.Customer.Name"),
                ElseExpression = CreatePath("$source.Orders")
            }, "Customer.Display")
        ],
        []);

        SemanticAnalysisResult result = analyzer.Analyze(document);

        AssertDiagnostic(result, "BMSM012");
    }

    // Creates the analyzer with real schema path resolution.
    private static TransformationSemanticAnalyzer CreateAnalyzer(IFunctionRegistry functionRegistry, IValidationRuleRegistry validationRuleRegistry)
    {
        return new TransformationSemanticAnalyzer(new SchemaPathResolver(), functionRegistry, validationRuleRegistry);
    }

    // Creates a document with default source and target schemas.
    private static ITransformationDocument CreateDocument(IReadOnlyCollection<ITransformationMapping> mappings, IReadOnlyCollection<IValidationRule> rules)
    {
        return new TransformationDocument
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["source"] = CreateSourceSchema()
            },
            TargetSchema = CreateTargetSchema(),
            Mappings = mappings,
            Validations = rules
        };
    }

    // Creates a source schema used by semantic tests.
    private static IStructureSchema CreateSourceSchema()
    {
        return new StructureSchema
        {
            Key = "source",
            Name = "Source",
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
                    },
                    new SchemaNode
                    {
                        Name = "Orders",
                        Kind = SchemaNodeKind.Array,
                        Children =
                        [
                            new SchemaNode
                            {
                                Name = "$item",
                                Kind = SchemaNodeKind.Object,
                                Children =
                                [
                                    new SchemaNode
                                    {
                                        Name = "Id",
                                        Kind = SchemaNodeKind.Scalar,
                                        DataType = "String"
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }
        };
    }

    // Creates a target schema used by semantic tests.
    private static IStructureSchema CreateTargetSchema()
    {
        return new StructureSchema
        {
            Key = "target",
            Name = "Target",
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
                            },
                            new SchemaNode
                            {
                                Name = "Display",
                                Kind = SchemaNodeKind.Scalar,
                                DataType = "String"
                            }
                        ]
                    },
                    new SchemaNode
                    {
                        Name = "OrderIds",
                        Kind = SchemaNodeKind.Array,
                        Children =
                        [
                            new SchemaNode
                            {
                                Name = "$item",
                                Kind = SchemaNodeKind.Scalar,
                                DataType = "String"
                            }
                        ]
                    }
                ]
            }
        };
    }

    // Creates a target schema with a numeric scalar.
    private static IStructureSchema CreateNumberTargetSchema()
    {
        return new StructureSchema
        {
            Key = "target",
            Name = "Target",
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
                                Name = "Amount",
                                Kind = SchemaNodeKind.Scalar,
                                DataType = "Number"
                            }
                        ]
                    }
                ]
            }
        };
    }

    // Creates a function registry with semantic descriptors.
    private static FunctionRegistry CreateFunctionRegistry()
    {
        FunctionRegistry registry = new();
        IFunction function = new CapturingFunction(new ScalarFunctionResult
        {
            Value = new ScalarValue()
        });
        registry.Register("concat", function, new FunctionDescriptor
        {
            Key = "concat",
            DisplayName = "Concat",
            Description = "Combines scalar values.",
            ValueKind = FunctionValueKind.Scalar,
            Parameters =
            [
                CreateFunctionParameter("left", FunctionValueKind.Scalar),
                CreateFunctionParameter("right", FunctionValueKind.Scalar)
            ]
        });
        registry.Register("nodeOnly", function, new FunctionDescriptor
        {
            Key = "nodeOnly",
            DisplayName = "Node Only",
            Description = "Accepts a structure node.",
            ValueKind = FunctionValueKind.Scalar,
            Parameters =
            [
                CreateFunctionParameter("node", FunctionValueKind.StructureNode)
            ]
        });
        registry.Register("split", function, new FunctionDescriptor
        {
            Key = "split",
            DisplayName = "Split",
            Description = "Splits text.",
            ValueKind = FunctionValueKind.ScalarCollection,
            Parameters =
            [
                CreateFunctionParameter("text", FunctionValueKind.Scalar),
                CreateFunctionParameter("separator", FunctionValueKind.Scalar)
            ]
        });
        registry.Register("toNumber", function, new FunctionDescriptor
        {
            Key = "toNumber",
            DisplayName = "To Number",
            Description = "Converts a scalar value.",
            ValueKind = FunctionValueKind.Scalar,
            Parameters =
            [
                CreateFunctionParameter("value", FunctionValueKind.Scalar)
            ]
        });
        registry.Register("camelCase", function, new FunctionDescriptor
        {
            Key = "camelCase",
            DisplayName = "Camel Case",
            Description = "Converts text casing.",
            ValueKind = FunctionValueKind.Scalar,
            Parameters =
            [
                CreateFunctionParameter("value", FunctionValueKind.Scalar)
            ]
        });
        registry.Register("ToUpper", function, new FunctionDescriptor
        {
            Key = "ToUpper",
            DisplayName = "To Upper",
            Description = "Converts text casing.",
            ValueKind = FunctionValueKind.Scalar,
            Parameters =
            [
                CreateFunctionParameter("value", FunctionValueKind.Scalar)
            ]
        });
        registry.Register("sum", function, new FunctionDescriptor
        {
            Key = "sum",
            DisplayName = "Sum",
            Description = "Sums scalar collection values.",
            ValueKind = FunctionValueKind.Scalar,
            Parameters =
            [
                CreateFunctionParameter("values", FunctionValueKind.ScalarCollection)
            ]
        });

        return registry;
    }

    // Creates a validation rule registry with semantic descriptors.
    private static ValidationRuleRegistry CreateValidationRegistry()
    {
        ValidationRuleRegistry registry = new();
        IValidationRuleHandler handler = new PassingValidationRuleHandler();
        registry.Register("required", handler, new ValidationRuleDescriptor
        {
            Key = "required",
            DisplayName = "Required",
            Description = "Requires a value.",
            ValueKind = FunctionValueKind.Scalar,
            Parameters = []
        });
        registry.Register("min", handler, new ValidationRuleDescriptor
        {
            Key = "min",
            DisplayName = "Minimum",
            Description = "Checks a minimum value.",
            ValueKind = FunctionValueKind.Scalar,
            Parameters =
            [
                CreateValidationParameter("value", FunctionValueKind.Scalar)
            ]
        });
        registry.Register("nodeRule", handler, new ValidationRuleDescriptor
        {
            Key = "nodeRule",
            DisplayName = "Node Rule",
            Description = "Checks a node.",
            ValueKind = FunctionValueKind.StructureNode,
            Parameters =
            [
                CreateValidationParameter("node", FunctionValueKind.StructureNode)
            ]
        });

        return registry;
    }

    // Creates a function parameter descriptor.
    private static IFunctionParameterDescriptor CreateFunctionParameter(string key, FunctionValueKind valueKind)
    {
        return new FunctionParameterDescriptor
        {
            Key = key,
            DisplayName = key,
            Description = key,
            ValueKind = valueKind,
            IsRequired = true
        };
    }

    // Creates a validation parameter descriptor.
    private static IValidationRuleParameterDescriptor CreateValidationParameter(string key, FunctionValueKind valueKind)
    {
        return new ValidationRuleParameterDescriptor
        {
            Key = key,
            DisplayName = key,
            Description = key,
            ValueKind = valueKind,
            IsRequired = true
        };
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

    // Creates a concat call expression.
    private static IFunctionCallExpression CreateConcatExpression()
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

    // Creates a function call expression.
    private static IFunctionCallExpression CreateFunction(string key, IReadOnlyCollection<ITransformationExpression> arguments)
    {
        return new FunctionCallExpression
        {
            FunctionKey = key,
            Arguments = arguments
        };
    }

    // Creates a projection expression.
    private static ICollectionProjectionExpression CreateProjectionExpression()
    {
        return new CollectionProjectionExpression
        {
            SourceExpression = CreatePath("$source.Orders"),
            ItemAlias = "order",
            BodyExpression = CreatePath("order.Id")
        };
    }

    // Creates a string scalar literal expression.
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

    // Creates a numeric scalar literal expression.
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

    // Creates a boolean scalar literal expression.
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

    // Confirms that a semantic analysis result contains a diagnostic code.
    private static void AssertDiagnostic(SemanticAnalysisResult result, string code)
    {
        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => string.Equals(diagnostic.Code, code, System.StringComparison.Ordinal));
        Assert.All(result.Diagnostics, diagnostic => Assert.Equal("Error", diagnostic.Severity));
    }
}
