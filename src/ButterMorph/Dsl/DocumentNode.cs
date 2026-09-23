namespace ButterMorph.Dsl;

// Holds top-level DSL declarations after syntax analysis.
internal sealed class DocumentNode : AstNode
{
    internal List<AssignmentNode> Assignments { get; } = [];

    internal List<ValidationNode> Validations { get; } = [];

    internal string ValidationPayloadAlias { get; set; } = "source";

    internal string ValidationSchemaKey { get; set; } = string.Empty;

    internal List<ValidationAssertionNode> ValidationAssertions { get; } = [];

    internal Dictionary<string, string> Metadata { get; } = new(StringComparer.Ordinal);
}
