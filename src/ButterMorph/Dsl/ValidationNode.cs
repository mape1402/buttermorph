namespace ButterMorph.Dsl;

// Represents one validation rule declaration parsed from a validate block.
internal sealed class ValidationNode : AstNode
{
    internal string Path { get; set; } = string.Empty;

    internal string RuleKey { get; set; } = string.Empty;

    internal List<AstNode> Arguments { get; } = [];
}
