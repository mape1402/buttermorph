namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a structure schema used by visual tooling and DSL compilation.
/// </summary>
public interface IStructureSchema
{
    /// <summary>
    /// Gets the schema name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the schema root node.
    /// </summary>
    ISchemaNode Root { get; }

    /// <summary>
    /// Gets UI and tooling metadata for the schema.
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
