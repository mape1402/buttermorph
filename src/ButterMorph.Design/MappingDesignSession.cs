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
        _document = new TransformationDocument
        {
            Definition = document.Definition,
            SourceSchemas = document.SourceSchemas,
            TargetSchema = document.TargetSchema,
            Mappings = document.Mappings,
            Validations = document.Validations,
            Metadata = document.Metadata
        };

        return Success();
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

        Dictionary<string, IStructureSchema> schemas = new(_document.SourceSchemas, StringComparer.Ordinal)
        {
            [key] = schema
        };
        _document.SourceSchemas = schemas;

        return Success();
    }

    /// <summary>
    /// Loads the target schema.
    /// </summary>
    /// <param name="schema">The target schema.</param>
    /// <returns>The operation result.</returns>
    public IMappingOperationResult LoadTargetSchema(IStructureSchema schema)
    {
        _document.TargetSchema = schema;
        return Success();
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
}
