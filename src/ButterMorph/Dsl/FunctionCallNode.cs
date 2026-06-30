namespace ButterMorph.Dsl;

// Represents a function call expression in the internal syntax tree.
internal sealed class FunctionCallNode : AstNode
{
    internal string FunctionKey { get; set; } = string.Empty;

    internal List<AstNode> Arguments { get; } = [];
}
