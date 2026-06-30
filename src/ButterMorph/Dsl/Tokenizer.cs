namespace ButterMorph.Dsl;

// Converts DSL text into a positioned token stream used by the syntax analyzer.
internal sealed class Tokenizer
{
    // Stores the full DSL content being scanned.
    private readonly string _content;

    // Tracks the current character index.
    private int _position;

    // Tracks the current line for diagnostics.
    private int _line = 1;

    // Tracks the current column for diagnostics.
    private int _column = 1;

    internal Tokenizer(string content)
    {
        _content = content;
    }

    internal IReadOnlyList<Token> Tokenize()
    {
        List<Token> tokens = [];

        while (!IsAtEnd())
        {
            SkipTrivia();

            if (IsAtEnd())
            {
                break;
            }

            tokens.Add(ReadToken());
        }

        tokens.Add(new Token(TokenKind.End, string.Empty, _line, _column));
        return tokens;
    }

    private Token ReadToken()
    {
        char current = Current;
        int line = _line;
        int column = _column;

        if (current == '{')
        {
            Advance();
            return new Token(TokenKind.LeftBrace, "{", line, column);
        }

        if (current == '}')
        {
            Advance();
            return new Token(TokenKind.RightBrace, "}", line, column);
        }

        if (current == '(')
        {
            Advance();
            return new Token(TokenKind.LeftParen, "(", line, column);
        }

        if (current == ')')
        {
            Advance();
            return new Token(TokenKind.RightParen, ")", line, column);
        }

        if (current == '[')
        {
            Advance();
            return new Token(TokenKind.LeftBracket, "[", line, column);
        }

        if (current == ']')
        {
            Advance();
            return new Token(TokenKind.RightBracket, "]", line, column);
        }

        if (current == ':')
        {
            Advance();
            return new Token(TokenKind.Colon, ":", line, column);
        }

        if (current == ',')
        {
            Advance();
            return new Token(TokenKind.Comma, ",", line, column);
        }

        if (current == '=' && Peek() == '>')
        {
            Advance();
            Advance();
            return new Token(TokenKind.Arrow, "=>", line, column);
        }

        if (current == '"')
        {
            return ReadString();
        }

        if (char.IsDigit(current) || current == '-')
        {
            return ReadNumber();
        }

        if (IsIdentifierStart(current))
        {
            return ReadIdentifierOrPath();
        }

        throw Error($"Unexpected character '{current}'.", line, column);
    }

    private Token ReadString()
    {
        int line = _line;
        int column = _column;
        Advance();
        System.Text.StringBuilder builder = new();

        while (!IsAtEnd() && Current != '"')
        {
            if (Current == '\\')
            {
                Advance();
                if (IsAtEnd())
                {
                    throw Error("Unterminated string escape.", line, column);
                }

                builder.Append(ReadEscapedCharacter());
                continue;
            }

            builder.Append(Current);
            Advance();
        }

        if (IsAtEnd())
        {
            throw Error("Unterminated string literal.", line, column);
        }

        Advance();
        return new Token(TokenKind.StringLiteral, builder.ToString(), line, column);
    }

    private char ReadEscapedCharacter()
    {
        char escaped = Current;
        Advance();

        if (escaped == 'n')
        {
            return '\n';
        }

        if (escaped == 'r')
        {
            return '\r';
        }

        if (escaped == 't')
        {
            return '\t';
        }

        if (escaped == '"' || escaped == '\\')
        {
            return escaped;
        }

        return escaped;
    }

    private Token ReadNumber()
    {
        int line = _line;
        int column = _column;
        int start = _position;

        if (Current == '-')
        {
            Advance();
        }

        while (!IsAtEnd() && char.IsDigit(Current))
        {
            Advance();
        }

        if (!IsAtEnd() && Current == '.')
        {
            Advance();

            while (!IsAtEnd() && char.IsDigit(Current))
            {
                Advance();
            }
        }

        return new Token(TokenKind.NumberLiteral, _content[start.._position], line, column);
    }

    private Token ReadIdentifierOrPath()
    {
        int line = _line;
        int column = _column;
        int start = _position;
        bool isPath = Current == '$';

        Advance();

        while (!IsAtEnd() && IsPathCharacter(Current))
        {
            if (Current == '.' || Current == '[' || Current == ']')
            {
                isPath = true;
            }

            Advance();
        }

        string value = _content[start.._position];
        TokenKind kind = TokenKind.Identifier;

        if (isPath)
        {
            kind = TokenKind.Path;
        }

        return new Token(kind, value, line, column);
    }

    private void SkipTrivia()
    {
        bool scanning = true;

        while (scanning && !IsAtEnd())
        {
            scanning = false;

            while (!IsAtEnd() && char.IsWhiteSpace(Current))
            {
                Advance();
                scanning = true;
            }

            if (!IsAtEnd() && Current == '#')
            {
                SkipLine();
                scanning = true;
            }

            if (!IsAtEnd() && Current == '/' && Peek() == '/')
            {
                SkipLine();
                scanning = true;
            }
        }
    }

    private void SkipLine()
    {
        while (!IsAtEnd() && Current != '\n')
        {
            Advance();
        }
    }

    private void Advance()
    {
        if (Current == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        _position++;
    }

    private char Peek()
    {
        int next = _position + 1;

        if (next >= _content.Length)
        {
            return '\0';
        }

        return _content[next];
    }

    private bool IsAtEnd()
    {
        return _position >= _content.Length;
    }

    private char Current => _content[_position];

    private static bool IsIdentifierStart(char value)
    {
        return char.IsLetter(value) || value == '_' || value == '$';
    }

    private static bool IsPathCharacter(char value)
    {
        return char.IsLetterOrDigit(value) || value == '_' || value == '$' || value == '.' || value == '[' || value == ']';
    }

    private static FormatException Error(string message, int line, int column)
    {
        return new FormatException($"{message} Line {line}, column {column}.");
    }
}
