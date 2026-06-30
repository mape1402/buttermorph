namespace ButterMorph.Dsl;

// Stores token text and position for parser diagnostics.
internal sealed class Token
{
    internal Token(TokenKind kind, string value, int line, int column)
    {
        Kind = kind;
        Value = value;
        Line = line;
        Column = column;
    }

    internal TokenKind Kind { get; }

    internal string Value { get; }

    internal int Line { get; }

    internal int Column { get; }
}
