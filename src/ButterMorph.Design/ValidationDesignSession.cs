namespace ButterMorph.Design;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Represents an editable validation design session.
/// </summary>
public sealed class ValidationDesignSession : IValidationDesignSession
{
    // Parses imported DSL content.
    private readonly IDslParser _dslParser;

    // Exports validation documents to DSL content.
    private readonly IValidationDslExporter _dslExporter;

    // Stores the current editable document.
    private ValidationDocument _document = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationDesignSession"/> class.
    /// </summary>
    /// <param name="dslParser">The DSL parser.</param>
    /// <param name="dslExporter">The validation DSL exporter.</param>
    public ValidationDesignSession(IDslParser dslParser, IValidationDslExporter dslExporter)
    {
        _dslParser = dslParser;
        _dslExporter = dslExporter;
    }

    /// <summary>
    /// Gets the current validation document.
    /// </summary>
    public IValidationDocument Document => _document;

    /// <summary>
    /// Loads an initial validation document.
    /// </summary>
    /// <param name="document">The validation document.</param>
    /// <returns>The operation result.</returns>
    public IValidationOperationResult LoadDocument(IValidationDocument document)
    {
        if (document == null)
        {
            return Failure("BVDG001", "Validation document is required.", string.Empty);
        }

        _document = CloneDocument(document);
        return Success();
    }

    /// <summary>
    /// Replaces the current validation document.
    /// </summary>
    /// <param name="payloadAlias">The payload alias used by scoped assertions.</param>
    /// <param name="schemaKey">The schema key used by scoped assertions.</param>
    /// <param name="rules">The validation rules.</param>
    /// <param name="assertions">The validation assertions.</param>
    /// <returns>The operation result.</returns>
    public IValidationOperationResult ReplaceDocument(
        string payloadAlias,
        string schemaKey,
        IReadOnlyCollection<IValidationRule> rules,
        IReadOnlyCollection<IValidationAssertion> assertions)
    {
        _document = new ValidationDocument
        {
            PayloadAlias = ResolvePayloadAlias(payloadAlias),
            SchemaKey = schemaKey?.Trim() ?? string.Empty,
            Rules = rules ?? [],
            Assertions = assertions ?? []
        };

        return Success();
    }

    /// <summary>
    /// Imports validation DSL content into the current session.
    /// </summary>
    /// <param name="dsl">The DSL content.</param>
    /// <returns>The operation result.</returns>
    public IValidationOperationResult ImportDsl(string dsl)
    {
        try
        {
            IDslDocument parsed = _dslParser.Parse(new DslDefinition
            {
                Content = dsl
            });

            if (parsed is not IValidationDocument validationDocument)
            {
                return Failure("BVDG002", "Imported DSL did not produce a validation document.", string.Empty);
            }

            _document = CloneDocument(validationDocument);
            return Success();
        }
        catch (FormatException exception)
        {
            return Failure("BVDG003", exception.Message, string.Empty);
        }
    }

    /// <summary>
    /// Exports the current validation document into DSL content.
    /// </summary>
    /// <returns>The DSL content.</returns>
    public string ExportDsl()
    {
        return _dslExporter.Export(_document);
    }

    // Creates a successful operation result.
    private static IValidationOperationResult Success()
    {
        return new ValidationOperationResult
        {
            Succeeded = true,
            Diagnostics = []
        };
    }

    // Creates a failed operation result.
    private static IValidationOperationResult Failure(string code, string message, string path)
    {
        return new ValidationOperationResult
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

    // Creates a mutable document copy.
    private static ValidationDocument CloneDocument(IValidationDocument document)
    {
        return new ValidationDocument
        {
            Definition = document.Definition,
            PayloadAlias = ResolvePayloadAlias(document.PayloadAlias),
            SchemaKey = document.SchemaKey,
            Rules = document.Rules,
            Assertions = document.Assertions
        };
    }

    // Resolves a normalized payload alias.
    private static string ResolvePayloadAlias(string payloadAlias)
    {
        if (string.IsNullOrWhiteSpace(payloadAlias))
        {
            return "source";
        }

        return payloadAlias.Trim().TrimStart('$');
    }
}
