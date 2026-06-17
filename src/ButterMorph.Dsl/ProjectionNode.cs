namespace ButterMorph.Dsl;

// Represents a collection projection expression in the internal syntax tree.
internal sealed class ProjectionNode : AstNode
{
    internal AstNode SourceExpression { get; set; } = new PathNode();

    internal string ItemAlias { get; set; } = string.Empty;

    internal AstNode BodyExpression { get; set; } = new PathNode();
}
