namespace ButterMorph.Dsl;

// Represents one boolean validation assertion parsed from a validate block.
internal sealed class ValidationAssertionNode : AstNode
{
    internal AstNode Expression { get; set; }

    internal string Message { get; set; } = string.Empty;

    internal string Path { get; set; } = string.Empty;
}
