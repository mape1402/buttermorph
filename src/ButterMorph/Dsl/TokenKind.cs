namespace ButterMorph.Dsl;

// Identifies each token shape recognized by the DSL tokenizer.
internal enum TokenKind
{
    Identifier,
    Path,
    StringLiteral,
    NumberLiteral,
    LeftBrace,
    RightBrace,
    LeftParen,
    RightParen,
    LeftBracket,
    RightBracket,
    Colon,
    Comma,
    Arrow,
    End
}
