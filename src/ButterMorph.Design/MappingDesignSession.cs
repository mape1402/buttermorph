namespace ButterMorph.Design;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Represents an editable mapping design session.
/// </summary>
public sealed class MappingDesignSession : IMappingDesignSession
{
    // Parses imported DSL content.
    private readonly IDslParser _dslParser;

    // Exports current documents to DSL content.
    private readonly IDslExporter _dslExporter;

    // Runs semantic analysis for the current document.
    private readonly ITransformationSemanticAnalyzer _semanticAnalyzer;

    // Stores the current editable document.
    private TransformationDocument _document = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingDesignSession"/> class.
    /// </summary>
    /// <param name="dslParser">The DSL parser.</param>
    /// <param name="dslExporter">The DSL exporter.</param>
    /// <param name="semanticAnalyzer">The semantic analyzer.</param>
    public MappingDesignSession(IDslParser dslParser, IDslExporter dslExporter, ITransformationSemanticAnalyzer semanticAnalyzer)
    {
        _dslParser = dslParser;
        _dslExporter = dslExporter;
        _semanticAnalyzer = semanticAnalyzer;
    }

    /// <summary>
    /// Gets the current transformation document.
    /// </summary>
    public ITransformationDocument Document => _document;

    /// <summary>
    /// Loads an initial transformation document.
    /// </summary>
    /// <param name="document">The transformation document.</param>
    /// <returns>The operation result.</returns>
    public IMappingOperationResult LoadDocument(ITransformationDocument document)
    {
        List<DiagnosticEntry> diagnostics = [];
        foreach (KeyValuePair<string, IStructureSchema> schema in document.SourceSchemas)
        {
            AddSchemaIdentityDiagnostics(diagnostics, schema.Key, schema.Value);
        }

        AddTargetSchemaIdentityDiagnostics(diagnostics, document.TargetSchema);
        AddDuplicateSchemaKeyDiagnostics(diagnostics, document.SourceSchemas);

        _document = new TransformationDocument
        {
            Definition = document.Definition,
            SourceSchemas = document.SourceSchemas,
            TargetSchema = document.TargetSchema,
            Mappings = document.Mappings,
            Validations = document.Validations,
            Metadata = document.Metadata
        };

        return ResultFromDiagnostics(diagnostics);
    }

    /// <summary>
    /// Loads a source schema.
    /// </summary>
    /// <param name="key">The source key.</param>
    /// <param name="schema">The source schema.</param>
    /// <returns>The operation result.</returns>
    public IMappingOperationResult LoadSourceSchema(string key, IStructureSchema schema)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Failure("BMDG001", "Source schema key cannot be blank.", string.Empty);
        }

        List<DiagnosticEntry> diagnostics = [];
        AddSchemaIdentityDiagnostics(diagnostics, key, schema);

        Dictionary<string, IStructureSchema> schemas = new(_document.SourceSchemas, StringComparer.Ordinal)
        {
            [key] = schema
        };

        AddDuplicateSchemaKeyDiagnostics(diagnostics, schemas);
        _document.SourceSchemas = schemas;

        return ResultFromDiagnostics(diagnostics);
    }

    /// <summary>
    /// Loads the target schema.
    /// </summary>
    /// <param name="schema">The target schema.</param>
    /// <returns>The operation result.</returns>
    public IMappingOperationResult LoadTargetSchema(IStructureSchema schema)
    {
        List<DiagnosticEntry> diagnostics = [];
        AddTargetSchemaIdentityDiagnostics(diagnostics, schema);
        _document.TargetSchema = schema;
        return ResultFromDiagnostics(diagnostics);
    }

    /// <summary>
    /// Adds a path mapping.
    /// </summary>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The operation result.</returns>
    public IMappingOperationResult AddPathMapping(string sourcePath, string targetPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return Failure("BMDG002", "Source path cannot be blank.", targetPath);
        }

        return AddMapping(new PathExpression
        {
            Path = sourcePath
        }, targetPath);
    }

    /// <summary>
    /// Adds a mapping from textual DSL expression content.
    /// </summary>
    /// <param name="expressionText">The expression text.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The operation result.</returns>
    public IMappingOperationResult AddExpressionTextMapping(string expressionText, string targetPath)
    {
        if (string.IsNullOrWhiteSpace(expressionText))
        {
            return Failure("BMDG006", "Expression cannot be blank.", targetPath);
        }

        try
        {
            IDslDocument parsed = _dslParser.Parse(new DslDefinition
            {
                Content = "target {" + Environment.NewLine + "  Value: " + expressionText + Environment.NewLine + "}"
            });

            if (parsed is not ITransformationDocument transformationDocument)
            {
                return Failure("BMDG004", "Expression DSL did not produce a transformation document.", targetPath);
            }

            foreach (ITransformationMapping mapping in transformationDocument.Mappings)
            {
                return AddMapping(mapping.SourceExpression, targetPath);
            }

            return Failure("BMDG006", "Expression DSL did not produce a mapping.", targetPath);
        }
        catch (FormatException exception)
        {
            return Failure("BMDG005", exception.Message, targetPath);
        }
    }

    /// <summary>
    /// Adds an expression mapping.
    /// </summary>
    /// <param name="expression">The source expression.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The operation result.</returns>
    public IMappingOperationResult AddMapping(ITransformationExpression expression, string targetPath)
    {
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return Failure("BMDG003", "Target path cannot be blank.", string.Empty);
        }

        List<ITransformationMapping> mappings = [.. _document.Mappings];
        mappings.Add(new TransformationMapping
        {
            SourceExpression = expression,
            TargetPath = targetPath
        });
        _document.Mappings = mappings;

        return Success();
    }

    /// <summary>
    /// Removes mappings by target path.
    /// </summary>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The operation result.</returns>
    public IMappingOperationResult RemoveMapping(string targetPath)
    {
        List<ITransformationMapping> mappings = [];

        foreach (ITransformationMapping mapping in _document.Mappings)
        {
            if (!string.Equals(mapping.TargetPath, targetPath, StringComparison.Ordinal))
            {
                mappings.Add(mapping);
            }
        }

        _document.Mappings = mappings;
        return Success();
    }

    /// <summary>
    /// Adds a validation rule.
    /// </summary>
    /// <param name="rule">The validation rule.</param>
    /// <returns>The operation result.</returns>
    public IMappingOperationResult AddValidationRule(IValidationRule rule)
    {
        List<IValidationRule> validations = [.. _document.Validations];
        validations.Add(rule);
        _document.Validations = validations;

        return Success();
    }

    /// <summary>
    /// Removes validation rules by path and key.
    /// </summary>
    /// <param name="path">The validation path.</param>
    /// <param name="ruleKey">The rule key.</param>
    /// <returns>The operation result.</returns>
    public IMappingOperationResult RemoveValidationRule(string path, string ruleKey)
    {
        List<IValidationRule> validations = [];

        foreach (IValidationRule rule in _document.Validations)
        {
            bool samePath = string.Equals(rule.Path, path, StringComparison.Ordinal);
            bool sameKey = string.Equals(rule.RuleKey, ruleKey, StringComparison.Ordinal);

            if (!samePath || !sameKey)
            {
                validations.Add(rule);
            }
        }

        _document.Validations = validations;
        return Success();
    }

    /// <summary>
    /// Imports DSL content into the current session.
    /// </summary>
    /// <param name="dsl">The DSL content.</param>
    /// <returns>The operation result.</returns>
    public IMappingOperationResult ImportDsl(string dsl)
    {
        try
        {
            IDslDocument parsed = _dslParser.Parse(new DslDefinition
            {
                Content = dsl
            });

            if (parsed is not ITransformationDocument transformationDocument)
            {
                return Failure("BMDG004", "Imported DSL did not produce a transformation document.", string.Empty);
            }

            _document = new TransformationDocument
            {
                Definition = transformationDocument.Definition,
                SourceSchemas = _document.SourceSchemas,
                TargetSchema = _document.TargetSchema,
                Mappings = transformationDocument.Mappings,
                Validations = transformationDocument.Validations,
                Metadata = transformationDocument.Metadata
            };

            return Success();
        }
        catch (FormatException exception)
        {
            return Failure("BMDG005", exception.Message, string.Empty);
        }
    }

    /// <summary>
    /// Exports the current document into DSL content.
    /// </summary>
    /// <returns>The DSL content.</returns>
    public string ExportDsl()
    {
        return _dslExporter.Export(_document);
    }

    /// <summary>
    /// Runs semantic analysis for the current document.
    /// </summary>
    /// <returns>The semantic analysis result.</returns>
    public SemanticAnalysisResult Analyze()
    {
        return _semanticAnalyzer.Analyze(_document);
    }

    // Creates a successful operation result.
    private static IMappingOperationResult Success()
    {
        return new MappingOperationResult
        {
            Succeeded = true,
            Diagnostics = []
        };
    }

    // Creates a successful operation result with non-blocking diagnostics.
    private static IMappingOperationResult ResultFromDiagnostics(IReadOnlyCollection<DiagnosticEntry> diagnostics)
    {
        return new MappingOperationResult
        {
            Succeeded = HasNoErrors(diagnostics),
            Diagnostics = diagnostics
        };
    }

    // Creates a failed operation result.
    private static IMappingOperationResult Failure(string code, string message, string path)
    {
        return new MappingOperationResult
        {
            Succeeded = false,
            Diagnostics =
            [
                new DiagnosticEntry
                {
                    Code = code,
                    Message = message,
                    Path = path,
                    Severity = "Error"
                }
            ]
        };
    }

    // Determines whether diagnostics contain blocking errors.
    private static bool HasNoErrors(IReadOnlyCollection<DiagnosticEntry> diagnostics)
    {
        foreach (DiagnosticEntry diagnostic in diagnostics)
        {
            if (string.Equals(diagnostic.Severity, "Error", StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    // Adds schema identity diagnostics for source schemas.
    private static void AddSchemaIdentityDiagnostics(List<DiagnosticEntry> diagnostics, string alias, IStructureSchema schema)
    {
        if (string.IsNullOrWhiteSpace(schema.Key))
        {
            diagnostics.Add(CreateDiagnostic("BMDG008", "Source schema canonical key cannot be blank.", alias, "Error"));
            return;
        }

        if (string.IsNullOrWhiteSpace(schema.Name))
        {
            diagnostics.Add(CreateDiagnostic("BMDG009", "Source schema name cannot be blank.", alias, "Error"));
        }

        if (string.IsNullOrWhiteSpace(schema.Version))
        {
            diagnostics.Add(CreateDiagnostic("BMDG014", "Source schema version cannot be blank.", alias, "Error"));
        }

        if (!string.Equals(alias, schema.Key, StringComparison.Ordinal))
        {
            diagnostics.Add(CreateDiagnostic("BMDG010", "Source alias differs from schema key. Paths use the alias, but schema identity uses the key.", alias, "Warning"));
        }
    }

    // Adds schema identity diagnostics for target schemas.
    private static void AddTargetSchemaIdentityDiagnostics(List<DiagnosticEntry> diagnostics, IStructureSchema schema)
    {
        if (string.IsNullOrWhiteSpace(schema.Key))
        {
            diagnostics.Add(CreateDiagnostic("BMDG012", "Target schema canonical key cannot be blank.", "target", "Error"));
        }

        if (string.IsNullOrWhiteSpace(schema.Name))
        {
            diagnostics.Add(CreateDiagnostic("BMDG013", "Target schema name cannot be blank.", "target", "Error"));
        }

        if (string.IsNullOrWhiteSpace(schema.Version))
        {
            diagnostics.Add(CreateDiagnostic("BMDG015", "Target schema version cannot be blank.", "target", "Error"));
        }
    }

    // Adds duplicate canonical schema key and version diagnostics.
    private static void AddDuplicateSchemaKeyDiagnostics(List<DiagnosticEntry> diagnostics, IReadOnlyDictionary<string, IStructureSchema> schemas)
    {
        Dictionary<string, string> aliasesBySchemaIdentity = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, IStructureSchema> pair in schemas)
        {
            if (string.IsNullOrWhiteSpace(pair.Value.Key) || string.IsNullOrWhiteSpace(pair.Value.Version))
            {
                continue;
            }

            string schemaIdentity = pair.Value.Key + "@" + pair.Value.Version;
            if (aliasesBySchemaIdentity.ContainsKey(schemaIdentity))
            {
                diagnostics.Add(CreateDiagnostic("BMDG011", "Multiple source schemas use the same canonical schema key and version.", schemaIdentity, "Warning"));
                continue;
            }

            aliasesBySchemaIdentity[schemaIdentity] = pair.Key;
        }
    }

    // Creates one design diagnostic entry.
    private static DiagnosticEntry CreateDiagnostic(string code, string message, string path, string severity)
    {
        return new DiagnosticEntry
        {
            Code = code,
            Message = message,
            Path = path,
            Severity = severity
        };
    }
}
