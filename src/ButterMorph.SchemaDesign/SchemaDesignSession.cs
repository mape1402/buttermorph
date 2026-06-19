namespace ButterMorph.SchemaDesign;

using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Json.Schema;

/// <summary>
/// Provides editable schema design behavior.
/// </summary>
public sealed class SchemaDesignSession : ISchemaDesignSession
{
    // Imports JSON Schema text into canonical schemas.
    private readonly IJsonSchemaImporter importer;

    // Exports canonical schemas as JSON Schema text.
    private readonly IJsonSchemaExporter exporter;

    // Holds the current editable schema.
    private StructureSchema schema = CreateEmptySchema();

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaDesignSession"/> class.
    /// </summary>
    /// <param name="importer">The JSON Schema importer.</param>
    /// <param name="exporter">The JSON Schema exporter.</param>
    public SchemaDesignSession(IJsonSchemaImporter importer, IJsonSchemaExporter exporter)
    {
        this.importer = importer;
        this.exporter = exporter;
    }

    /// <summary>
    /// Gets the current schema.
    /// </summary>
    public IStructureSchema Schema => schema;

    /// <summary>
    /// Loads an existing schema into the session.
    /// </summary>
    /// <param name="schema">The schema to load.</param>
    /// <returns>The operation result.</returns>
    public ISchemaDesignOperationResult Load(IStructureSchema schema)
    {
        if (schema == null)
        {
            return Fail("BMSD001", "Schema is required.", "$root");
        }

        this.schema = CloneSchema(schema);

        return Success();
    }

    /// <summary>
    /// Imports JSON Schema text into the session.
    /// </summary>
    /// <param name="name">The schema name.</param>
    /// <param name="jsonSchema">The JSON Schema text.</param>
    /// <returns>The operation result.</returns>
    public ISchemaDesignOperationResult ImportJsonSchema(string name, string jsonSchema)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Fail("BMSD002", "Schema name is required.", "$root");
        }

        if (string.IsNullOrWhiteSpace(jsonSchema))
        {
            return Fail("BMSD003", "JSON Schema text is required.", "$root");
        }

        JsonSchemaConversionResult result = importer.Import(new JsonSchemaImportRequest
        {
            Name = name,
            JsonSchema = jsonSchema
        });

        if (!result.Succeeded)
        {
            return new SchemaDesignOperationResult
            {
                Succeeded = false,
                Diagnostics = result.Diagnostics
            };
        }

        schema = CloneSchema(result.Schema);

        return Success();
    }

    /// <summary>
    /// Exports the current schema as JSON Schema text.
    /// </summary>
    /// <returns>The exported JSON Schema text.</returns>
    public string ExportJsonSchema()
    {
        JsonSchemaConversionResult result = exporter.Export(new JsonSchemaExportRequest
        {
            Schema = schema
        });

        return result.JsonSchema;
    }

    /// <summary>
    /// Adds a node under the selected parent path.
    /// </summary>
    /// <param name="parentPath">The parent path.</param>
    /// <param name="name">The new node name.</param>
    /// <param name="kind">The new node kind.</param>
    /// <param name="dataType">The scalar data type.</param>
    /// <returns>The operation result.</returns>
    public ISchemaDesignOperationResult AddNode(string parentPath, string name, SchemaNodeKind kind, string dataType)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Fail("BMSD004", "Node name is required.", parentPath);
        }

        SchemaNode parent = FindNode(parentPath);

        if (parent == null)
        {
            return Fail("BMSD005", "Parent path was not found.", parentPath);
        }

        if (parent.Kind == SchemaNodeKind.Scalar)
        {
            return Fail("BMSD006", "Scalar nodes cannot contain children.", parentPath);
        }

        List<ISchemaNode> children = parent.Children.ToList();

        if (children.Any(child => string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            return Fail("BMSD007", "A node with the same name already exists.", parentPath);
        }

        children.Add(CreateNode(name, kind, dataType, false));
        parent.Children = children;

        return Success();
    }

    /// <summary>
    /// Updates a node.
    /// </summary>
    /// <param name="path">The node path.</param>
    /// <param name="name">The node name.</param>
    /// <param name="kind">The node kind.</param>
    /// <param name="dataType">The scalar data type.</param>
    /// <param name="isRequired">A value indicating whether the node is required.</param>
    /// <returns>The operation result.</returns>
    public ISchemaDesignOperationResult UpdateNode(string path, string name, SchemaNodeKind kind, string dataType, bool isRequired)
    {
        SchemaNode node = FindNode(path);

        if (node == null)
        {
            return Fail("BMSD008", "Node path was not found.", path);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Fail("BMSD009", "Node name is required.", path);
        }

        if (string.Equals(GetLastSegment(path), "$item", StringComparison.Ordinal))
        {
            name = "$item";
        }

        node.Name = name;

        if (string.Equals(path, "$root", StringComparison.Ordinal))
        {
            node.Name = "$root";
        }

        node.Kind = kind;
        node.DataType = string.Empty;

        if (kind == SchemaNodeKind.Scalar)
        {
            node.DataType = NormalizeDataType(dataType);
        }
        node.IsRequired = isRequired;

        if (kind == SchemaNodeKind.Scalar)
        {
            node.Children = [];
        }
        else if (kind == SchemaNodeKind.Array && !node.Children.Any(child => string.Equals(child.Name, "$item", StringComparison.Ordinal)))
        {
            node.Children = [CreateNode("$item", SchemaNodeKind.Scalar, NormalizeDataType(dataType), false)];
        }

        return Success();
    }

    /// <summary>
    /// Removes a node.
    /// </summary>
    /// <param name="path">The node path.</param>
    /// <returns>The operation result.</returns>
    public ISchemaDesignOperationResult RemoveNode(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || string.Equals(path, "$root", StringComparison.Ordinal))
        {
            return Fail("BMSD010", "Root node cannot be removed.", "$root");
        }

        SchemaNode parent = FindParentNode(path);

        if (parent == null)
        {
            return Fail("BMSD011", "Parent path was not found.", path);
        }

        string nodeName = GetLastSegment(path);
        List<ISchemaNode> children = parent.Children.Where(child => !string.Equals(child.Name, nodeName, StringComparison.Ordinal)).ToList();

        if (children.Count == parent.Children.Count)
        {
            return Fail("BMSD012", "Node path was not found.", path);
        }

        parent.Children = children;

        return Success();
    }

    /// <summary>
    /// Sets metadata on a schema node.
    /// </summary>
    /// <param name="path">The node path.</param>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The operation result.</returns>
    public ISchemaDesignOperationResult SetMetadata(string path, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Fail("BMSD013", "Metadata key is required.", path);
        }

        if (string.IsNullOrWhiteSpace(path) || string.Equals(path, "$root", StringComparison.Ordinal))
        {
            Dictionary<string, string> metadata = new(schema.Metadata, StringComparer.Ordinal);
            metadata[key] = value;

            if (value == null)
            {
                metadata[key] = string.Empty;
            }
            schema.Metadata = metadata;

            return Success();
        }

        SchemaNode node = FindNode(path);

        if (node == null)
        {
            return Fail("BMSD014", "Node path was not found.", path);
        }

        Dictionary<string, string> nodeMetadata = new(node.Metadata, StringComparer.Ordinal);
        nodeMetadata[key] = value;

        if (value == null)
        {
            nodeMetadata[key] = string.Empty;
        }
        node.Metadata = nodeMetadata;

        return Success();
    }

    // Creates an empty editable schema.
    private static StructureSchema CreateEmptySchema()
    {
        return new StructureSchema
        {
            Name = "Schema",
            Root = new SchemaNode
            {
                Name = "$root",
                Kind = SchemaNodeKind.Object,
                Children = []
            },
            Metadata = new Dictionary<string, string>()
        };
    }

    // Creates a successful operation result.
    private static ISchemaDesignOperationResult Success()
    {
        return new SchemaDesignOperationResult
        {
            Succeeded = true,
            Diagnostics = []
        };
    }

    // Creates a failed operation result with one diagnostic.
    private static ISchemaDesignOperationResult Fail(string code, string message, string path)
    {
        return new SchemaDesignOperationResult
        {
            Succeeded = false,
            Diagnostics =
            [
                new DiagnosticEntry
                {
                    Code = code,
                    Message = message,
                    Severity = "Error",
                    Path = path
                }
            ]
        };
    }

    // Creates a schema node with array item support.
    private static SchemaNode CreateNode(string name, SchemaNodeKind kind, string dataType, bool isRequired)
    {
        List<ISchemaNode> children = [];

        if (kind == SchemaNodeKind.Array)
        {
            children.Add(new SchemaNode
            {
                Name = "$item",
                Kind = SchemaNodeKind.Scalar,
                DataType = NormalizeDataType(dataType),
                IsRequired = false,
                Children = [],
                Metadata = new Dictionary<string, string>()
            });
        }

        return new SchemaNode
        {
            Name = name,
            Kind = kind,
            DataType = CreateNodeDataType(kind, dataType),
            IsRequired = isRequired,
            Children = children,
            Metadata = new Dictionary<string, string>()
        };
    }

    // Creates a detached mutable copy of a schema.
    private static StructureSchema CloneSchema(IStructureSchema source)
    {
        return new StructureSchema
        {
            Name = source.Name,
            Root = CloneNode(source.Root),
            Metadata = new Dictionary<string, string>(source.Metadata, StringComparer.Ordinal)
        };
    }

    // Creates a detached mutable copy of a schema node.
    private static SchemaNode CloneNode(ISchemaNode source)
    {
        return new SchemaNode
        {
            Name = source.Name,
            Kind = source.Kind,
            DataType = source.DataType,
            IsRequired = source.IsRequired,
            Children = source.Children.Select(CloneNode).ToList(),
            Metadata = new Dictionary<string, string>(source.Metadata, StringComparer.Ordinal)
        };
    }

    // Normalizes empty scalar data types.
    private static string NormalizeDataType(string dataType)
    {
        if (string.IsNullOrWhiteSpace(dataType))
        {
            return "string";
        }

        return dataType;
    }

    // Finds a mutable node by path.
    private SchemaNode FindNode(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || string.Equals(path, "$root", StringComparison.Ordinal))
        {
            return (SchemaNode)schema.Root;
        }

        SchemaNode current = (SchemaNode)schema.Root;

        foreach (string segment in NormalizePath(path))
        {
            SchemaNode next = current.Children.OfType<SchemaNode>().FirstOrDefault(child => string.Equals(child.Name, segment, StringComparison.Ordinal));

            if (next == null)
            {
                return null;
            }

            current = next;
        }

        return current;
    }

    // Finds the parent node for a path.
    private SchemaNode FindParentNode(string path)
    {
        List<string> segments = NormalizePath(path);

        if (segments.Count == 0)
        {
            return null;
        }

        segments.RemoveAt(segments.Count - 1);

        return FindNode(string.Join(".", segments));
    }

    // Gets the final segment from a schema path.
    private static string GetLastSegment(string path)
    {
        List<string> segments = NormalizePath(path);

        if (segments.Count == 0)
        {
            return "$root";
        }

        return segments[segments.Count - 1];
    }

    // Creates the node data type value for the requested kind.
    private static string CreateNodeDataType(SchemaNodeKind kind, string dataType)
    {
        if (kind == SchemaNodeKind.Scalar)
        {
            return NormalizeDataType(dataType);
        }

        return string.Empty;
    }

    // Converts UI paths into internal node names.
    private static List<string> NormalizePath(string path)
    {
        string normalized = path.Replace("$root.", string.Empty, StringComparison.Ordinal)
            .Replace("$root", string.Empty, StringComparison.Ordinal)
            .Replace("[0]", ".$item", StringComparison.Ordinal)
            .Trim('.');

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return [];
        }

        return normalized.Split('.', StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
