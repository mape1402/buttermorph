namespace ButterMorph.Dsl;

// Represents validation statements parsed from a foreach validation block.
internal sealed class ValidationForEachNode : AstNode
{
    internal AstNode SourceExpression { get; set; }

    internal string ItemAlias { get; set; } = "item";

    internal List<AstNode> Statements { get; } = [];
}
