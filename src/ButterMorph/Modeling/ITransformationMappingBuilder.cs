namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;

/// <summary>
/// Builds transformation mappings.
/// </summary>
public interface ITransformationMappingBuilder
{
    /// <summary>
    /// Sets the source expression.
    /// </summary>
    /// <param name="expression">The source expression.</param>
    /// <returns>The current builder.</returns>
    ITransformationMappingBuilder From(ITransformationExpression expression);

    /// <summary>
    /// Sets a source path expression.
    /// </summary>
    /// <param name="path">The source path.</param>
    /// <returns>The current builder.</returns>
    ITransformationMappingBuilder FromPath(string path);

    /// <summary>
    /// Sets the target path.
    /// </summary>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The current builder.</returns>
    ITransformationMappingBuilder To(string targetPath);

    /// <summary>
    /// Builds the transformation mapping.
    /// </summary>
    /// <returns>The transformation mapping.</returns>
    ITransformationMapping Build();
}
