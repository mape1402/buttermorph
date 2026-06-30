namespace ButterMorph.Dsl;

// Represents a conditional expression in the internal syntax tree.
internal sealed class ConditionNode : AstNode
{
    internal AstNode Condition { get; set; } = new PathNode();

    internal AstNode ThenExpression { get; set; } = new PathNode();

    internal AstNode ElseExpression { get; set; } = new PathNode();
}
