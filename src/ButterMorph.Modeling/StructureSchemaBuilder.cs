namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Builds structure schemas.
/// </summary>
public sealed class StructureSchemaBuilder : IStructureSchemaBuilder
{
    // Stores the canonical schema key.
    private string _key;

    // Stores the schema name.
    private readonly string _name;

    // Stores the schema description.
    private string _description = string.Empty;

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
        _key = CreateKey(name);
    }

    /// <summary>
    /// Sets the canonical schema key.
    /// </summary>
    /// <param name="key">The schema key.</param>
    /// <returns>The current builder.</returns>
    public IStructureSchemaBuilder WithKey(string key)
    {
        Guard.NotBlank(key, nameof(key));
        _key = key.Trim();
        return this;
    }

    /// <summary>
    /// Sets the schema description.
    /// </summary>
    /// <param name="description">The schema description.</param>
    /// <returns>The current builder.</returns>
    public IStructureSchemaBuilder WithDescription(string description)
    {
        _description = description;
        return this;
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
        Guard.NotBlank(_key, nameof(_key));
        Guard.NotBlank(_name, nameof(_name));
        return new StructureSchema
        {
            Key = _key,
            Name = _name,
            Description = _description,
            Root = _root,
            Metadata = new Dictionary<string, string>(_metadata, StringComparer.Ordinal)
        };
    }

    // Creates a stable key from display text.
    private static string CreateKey(string name)
    {
        string text = name.Trim();
        List<char> characters = [];
        bool lastWasSeparator = false;

        foreach (char character in text)
        {
            if (char.IsLetterOrDigit(character))
            {
                characters.Add(char.ToLowerInvariant(character));
                lastWasSeparator = false;
                continue;
            }

            if (!lastWasSeparator && characters.Count > 0)
            {
                characters.Add('-');
                lastWasSeparator = true;
            }
        }

        if (characters.Count > 0 && characters[^1] == '-')
        {
            characters.RemoveAt(characters.Count - 1);
        }

        string key = new(characters.ToArray());
        Guard.NotBlank(key, nameof(name));
        return key;
    }
}
