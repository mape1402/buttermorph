namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Builds transformation mappings.
/// </summary>
public sealed class TransformationMappingBuilder : ITransformationMappingBuilder
{
    // Stores the source expression configured for the mapping.
    private ITransformationExpression _sourceExpression = new PathExpression();

    // Stores the target path configured for the mapping.
    private string _targetPath = string.Empty;

    /// <summary>
    /// Sets the source expression.
    /// </summary>
    /// <param name="expression">The source expression.</param>
    /// <returns>The current builder.</returns>
    public ITransformationMappingBuilder From(ITransformationExpression expression)
    {
        _sourceExpression = expression;
        return this;
    }

    /// <summary>
    /// Sets a source path expression.
    /// </summary>
    /// <param name="path">The source path.</param>
    /// <returns>The current builder.</returns>
    public ITransformationMappingBuilder FromPath(string path)
    {
        _sourceExpression = new ExpressionBuilder().Path(path);
        return this;
    }

    /// <summary>
    /// Sets the target path.
    /// </summary>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The current builder.</returns>
    public ITransformationMappingBuilder To(string targetPath)
    {
        Guard.NotBlank(targetPath, nameof(targetPath));
        _targetPath = targetPath;
        return this;
    }

    /// <summary>
    /// Builds the transformation mapping.
    /// </summary>
    /// <returns>The transformation mapping.</returns>
    public ITransformationMapping Build()
    {
        Guard.NotBlank(_targetPath, nameof(_targetPath));

        return new TransformationMapping
        {
            SourceExpression = _sourceExpression,
            TargetPath = _targetPath
        };
    }
}
