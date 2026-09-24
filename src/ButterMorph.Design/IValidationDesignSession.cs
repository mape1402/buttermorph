namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Represents an editable validation design session.
/// </summary>
public interface IValidationDesignSession
{
    /// <summary>
    /// Gets the current validation document.
    /// </summary>
    IValidationDocument Document { get; }

    /// <summary>
    /// Loads an initial validation document.
    /// </summary>
    /// <param name="document">The validation document.</param>
    /// <returns>The operation result.</returns>
    IValidationOperationResult LoadDocument(IValidationDocument document);

    /// <summary>
    /// Replaces the current validation document.
    /// </summary>
    /// <param name="payloadAlias">The payload alias used by scoped assertions.</param>
    /// <param name="schemaKey">The schema key used by scoped assertions.</param>
    /// <param name="rules">The validation rules.</param>
    /// <param name="assertions">The validation assertions.</param>
    /// <returns>The operation result.</returns>
    IValidationOperationResult ReplaceDocument(
        string payloadAlias,
        string schemaKey,
        IReadOnlyCollection<IValidationRule> rules,
        IReadOnlyCollection<IValidationAssertion> assertions);

    /// <summary>
    /// Imports validation DSL content into the current session.
    /// </summary>
    /// <param name="dsl">The DSL content.</param>
    /// <returns>The operation result.</returns>
    IValidationOperationResult ImportDsl(string dsl);

    /// <summary>
    /// Exports the current validation document into DSL content.
    /// </summary>
    /// <returns>The DSL content.</returns>
    string ExportDsl();
}
