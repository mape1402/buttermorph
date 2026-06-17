namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Represents an editable mapping design session.
/// </summary>
public interface IMappingDesignSession
{
    /// <summary>
    /// Gets the current transformation document.
    /// </summary>
    ITransformationDocument Document { get; }

    /// <summary>
    /// Loads a source schema.
    /// </summary>
    /// <param name="key">The source key.</param>
    /// <param name="schema">The source schema.</param>
    /// <returns>The operation result.</returns>
    IMappingOperationResult LoadSourceSchema(string key, IStructureSchema schema);

    /// <summary>
    /// Loads the target schema.
    /// </summary>
    /// <param name="schema">The target schema.</param>
    /// <returns>The operation result.</returns>
    IMappingOperationResult LoadTargetSchema(IStructureSchema schema);

    /// <summary>
    /// Adds a path mapping.
    /// </summary>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The operation result.</returns>
    IMappingOperationResult AddPathMapping(string sourcePath, string targetPath);

    /// <summary>
    /// Adds a mapping from textual DSL expression content.
    /// </summary>
    /// <param name="expressionText">The expression text.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The operation result.</returns>
    IMappingOperationResult AddExpressionTextMapping(string expressionText, string targetPath);

    /// <summary>
    /// Adds an expression mapping.
    /// </summary>
    /// <param name="expression">The source expression.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The operation result.</returns>
    IMappingOperationResult AddMapping(ITransformationExpression expression, string targetPath);

    /// <summary>
    /// Removes mappings by target path.
    /// </summary>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The operation result.</returns>
    IMappingOperationResult RemoveMapping(string targetPath);

    /// <summary>
    /// Adds a validation rule.
    /// </summary>
    /// <param name="rule">The validation rule.</param>
    /// <returns>The operation result.</returns>
    IMappingOperationResult AddValidationRule(IValidationRule rule);

    /// <summary>
    /// Removes validation rules by path and key.
    /// </summary>
    /// <param name="path">The validation path.</param>
    /// <param name="ruleKey">The rule key.</param>
    /// <returns>The operation result.</returns>
    IMappingOperationResult RemoveValidationRule(string path, string ruleKey);

    /// <summary>
    /// Imports DSL content into the current session.
    /// </summary>
    /// <param name="dsl">The DSL content.</param>
    /// <returns>The operation result.</returns>
    IMappingOperationResult ImportDsl(string dsl);

    /// <summary>
    /// Exports the current document into DSL content.
    /// </summary>
    /// <returns>The DSL content.</returns>
    string ExportDsl();

    /// <summary>
    /// Runs semantic analysis for the current document.
    /// </summary>
    /// <returns>The semantic analysis result.</returns>
    SemanticAnalysisResult Analyze();
}
