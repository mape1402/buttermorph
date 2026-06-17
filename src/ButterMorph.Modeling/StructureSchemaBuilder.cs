namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Builds structure schemas.
/// </summary>
public sealed class StructureSchemaBuilder : IStructureSchemaBuilder
{
    // Stores the schema name.
    private readonly string _name;

    // Stores the root schema node.
    private ISchemaNode _root = new SchemaNode
    {
        Name = "$root",
        Kind = SchemaNodeKind.Object,
        Children = []
    };

    // Stores schema metadata.
    private readonly Dictionary<string, string> _metadata = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="StructureSchemaBuilder"/> class.
    /// </summary>
    /// <param name="name">The schema name.</param>
    public StructureSchemaBuilder(string name)
    {
        Guard.NotBlank(name, nameof(name));
        _name = name;
    }

    /// <summary>
    /// Sets the schema root.
    /// </summary>
    /// <param name="root">The schema root.</param>
    /// <returns>The current builder.</returns>
    public IStructureSchemaBuilder WithRoot(ISchemaNode root)
    {
        _root = root;
        return this;
    }

    /// <summary>
    /// Adds schema metadata.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current builder.</returns>
    public IStructureSchemaBuilder WithMetadata(string key, string value)
    {
        Guard.NotBlank(key, nameof(key));
        _metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Builds the structure schema.
    /// </summary>
    /// <returns>The structure schema.</returns>
    public IStructureSchema Build()
    {
        return new StructureSchema
        {
            Name = _name,
            Root = _root,
            Metadata = new Dictionary<string, string>(_metadata, StringComparer.Ordinal)
        };
    }
}
