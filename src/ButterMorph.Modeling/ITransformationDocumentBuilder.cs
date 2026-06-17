namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;

/// <summary>
/// Builds transformation documents.
/// </summary>
public interface ITransformationDocumentBuilder
{
    /// <summary>
    /// Sets the source DSL definition.
    /// </summary>
    /// <param name="definition">The DSL definition.</param>
    /// <returns>The current builder.</returns>
    ITransformationDocumentBuilder WithDefinition(IDslDefinition definition);

    /// <summary>
    /// Adds a source schema.
    /// </summary>
    /// <param name="key">The source key.</param>
    /// <param name="schema">The source schema.</param>
    /// <returns>The current builder.</returns>
    ITransformationDocumentBuilder WithSourceSchema(string key, IStructureSchema schema);

    /// <summary>
    /// Sets the target schema.
    /// </summary>
    /// <param name="schema">The target schema.</param>
    /// <returns>The current builder.</returns>
    ITransformationDocumentBuilder WithTargetSchema(IStructureSchema schema);

    /// <summary>
    /// Adds a transformation mapping.
    /// </summary>
    /// <param name="expression">The source expression.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The current builder.</returns>
    ITransformationDocumentBuilder Map(ITransformationExpression expression, string targetPath);

    /// <summary>
    /// Adds a path transformation mapping.
    /// </summary>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The current builder.</returns>
    ITransformationDocumentBuilder MapPath(string sourcePath, string targetPath);

    /// <summary>
    /// Adds a validation rule.
    /// </summary>
    /// <param name="rule">The validation rule.</param>
    /// <returns>The current builder.</returns>
    ITransformationDocumentBuilder WithValidation(IValidationRule rule);

    /// <summary>
    /// Adds document metadata.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current builder.</returns>
    ITransformationDocumentBuilder WithMetadata(string key, string value);

    /// <summary>
    /// Builds the transformation document.
    /// </summary>
    /// <returns>The transformation document.</returns>
    ITransformationDocument Build();
}
