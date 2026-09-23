namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a validation execution request.
/// </summary>
public sealed class ValidationRequest
{
    /// <summary>
    /// Gets or sets the graph to validate.
    /// </summary>
    public IStructureGraph SourceGraph { get; set; }

    /// <summary>
    /// Gets or sets the payload alias exposed to validation expressions.
    /// </summary>
    public string PayloadAlias { get; set; } = "source";

    /// <summary>
    /// Gets or sets all payload graphs available to validation expressions.
    /// </summary>
    public IReadOnlyDictionary<string, IStructureGraph> Sources { get; set; } = new Dictionary<string, IStructureGraph>();

    /// <summary>
    /// Gets or sets the schema used to validate the payload graph.
    /// </summary>
    public IStructureSchema Schema { get; set; }

    /// <summary>
    /// Gets or sets all schemas available to validation orchestration.
    /// </summary>
    public IReadOnlyDictionary<string, IStructureSchema> Schemas { get; set; } = new Dictionary<string, IStructureSchema>();

    /// <summary>
    /// Gets or sets the parsed validation definition.
    /// </summary>
    public IDslDocument Definition { get; set; }
}
