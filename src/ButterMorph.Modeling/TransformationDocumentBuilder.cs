namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Builds transformation documents.
/// </summary>
public sealed class TransformationDocumentBuilder : ITransformationDocumentBuilder
{
    // Stores the optional DSL definition associated with the document.
    private IDslDefinition _definition = new DslDefinition();

    // Stores source schemas by source key.
    private readonly Dictionary<string, IStructureSchema> _sourceSchemas = new(StringComparer.Ordinal);

    // Stores the target schema for the document.
    private IStructureSchema _targetSchema = new StructureSchema();

    // Stores transformation mappings in insertion order.
    private readonly List<ITransformationMapping> _mappings = [];

    // Stores validation rules in insertion order.
    private readonly List<IValidationRule> _validations = [];

    // Stores document metadata.
    private readonly Dictionary<string, string> _metadata = new(StringComparer.Ordinal);

    /// <summary>
    /// Sets the source DSL definition.
    /// </summary>
    /// <param name="definition">The DSL definition.</param>
    /// <returns>The current builder.</returns>
    public ITransformationDocumentBuilder WithDefinition(IDslDefinition definition)
    {
        _definition = definition;
        return this;
    }

    /// <summary>
    /// Adds a source schema.
    /// </summary>
    /// <param name="key">The source key.</param>
    /// <param name="schema">The source schema.</param>
    /// <returns>The current builder.</returns>
    public ITransformationDocumentBuilder WithSourceSchema(string key, IStructureSchema schema)
    {
        Guard.NotBlank(key, nameof(key));
        _sourceSchemas[key] = schema;
        return this;
    }

    /// <summary>
    /// Sets the target schema.
    /// </summary>
    /// <param name="schema">The target schema.</param>
    /// <returns>The current builder.</returns>
    public ITransformationDocumentBuilder WithTargetSchema(IStructureSchema schema)
    {
        _targetSchema = schema;
        return this;
    }

    /// <summary>
    /// Adds a transformation mapping.
    /// </summary>
    /// <param name="expression">The source expression.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The current builder.</returns>
    public ITransformationDocumentBuilder Map(ITransformationExpression expression, string targetPath)
    {
        Guard.NotBlank(targetPath, nameof(targetPath));

        _mappings.Add(new TransformationMapping
        {
            SourceExpression = expression,
            TargetPath = targetPath
        });

        return this;
    }

    /// <summary>
    /// Adds a path transformation mapping.
    /// </summary>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The current builder.</returns>
    public ITransformationDocumentBuilder MapPath(string sourcePath, string targetPath)
    {
        return Map(new ExpressionBuilder().Path(sourcePath), targetPath);
    }

    /// <summary>
    /// Adds a validation rule.
    /// </summary>
    /// <param name="rule">The validation rule.</param>
    /// <returns>The current builder.</returns>
    public ITransformationDocumentBuilder WithValidation(IValidationRule rule)
    {
        _validations.Add(rule);
        return this;
    }

    /// <summary>
    /// Adds document metadata.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current builder.</returns>
    public ITransformationDocumentBuilder WithMetadata(string key, string value)
    {
        Guard.NotBlank(key, nameof(key));
        _metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Builds the transformation document.
    /// </summary>
    /// <returns>The transformation document.</returns>
    public ITransformationDocument Build()
    {
        return new TransformationDocument
        {
            Definition = _definition,
            SourceSchemas = new Dictionary<string, IStructureSchema>(_sourceSchemas, StringComparer.Ordinal),
            TargetSchema = _targetSchema,
            Mappings = [.. _mappings],
            Validations = [.. _validations],
            Metadata = new Dictionary<string, string>(_metadata, StringComparer.Ordinal)
        };
    }
}
