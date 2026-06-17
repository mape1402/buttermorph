namespace ButterMorph.Dsl;

// Converts positioned tokens into an internal syntax tree.
internal sealed class SyntaxAnalyzer
{
    // Stores the token stream produced by the tokenizer.
    private readonly IReadOnlyList<Token> _tokens;

    // Tracks the current token index.
    private int _position;

    internal SyntaxAnalyzer(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
    }

    internal DocumentNode Analyze()
    {
        DocumentNode document = new();

        while (!IsAtEnd())
        {
            if (MatchIdentifier("target"))
            {
                ParseTargetBlock(document);
                continue;
            }

            if (MatchIdentifier("validate"))
            {
                ParseValidationBlock(document);
                continue;
            }

            if (MatchIdentifier("metadata"))
            {
                ParseMetadataBlock(document);
                continue;
            }

            throw Error(Current, "Expected target, validate, or metadata block.");
        }

        return document;
    }

    private void ParseTargetBlock(DocumentNode document)
    {
        Consume(TokenKind.LeftBrace, "Expected target block start.");
        ParseTargetEntries(document, string.Empty);
        Consume(TokenKind.RightBrace, "Expected target block end.");
    }

    private void ParseTargetEntries(DocumentNode document, string prefix)
    {
        while (!Check(TokenKind.RightBrace) && !IsAtEnd())
        {
            Token name = ConsumeTargetPath("Expected target member name.");
            string targetPath = CombinePath(prefix, name.Value);

            if (Check(TokenKind.LeftBrace) && name.Kind == TokenKind.Identifier)
            {
                Advance();
                ParseTargetEntries(document, targetPath);
                Consume(TokenKind.RightBrace, "Expected nested target block end.");
            }
            else
            {
                Consume(TokenKind.Colon, "Expected ':' after target member name.");
                AstNode expression = ParseExpression();
                document.Assignments.Add(new AssignmentNode
                {
                    TargetPath = targetPath,
                    Expression = expression
                });
            }

            Match(TokenKind.Comma);
        }
    }

    private void ParseValidationBlock(DocumentNode document)
    {
        Consume(TokenKind.LeftBrace, "Expected validate block start.");

        while (!Check(TokenKind.RightBrace) && !IsAtEnd())
        {
            Token path = ConsumePathLike("Expected validation path.");
            Consume(TokenKind.Colon, "Expected ':' after validation path.");
            Token rule = Consume(TokenKind.Identifier, "Expected validation rule key.");
            ValidationNode node = new()
            {
                Path = path.Value,
                RuleKey = rule.Value
            };

            if (Match(TokenKind.LeftParen))
            {
                ParseArguments(node.Arguments);
                Consume(TokenKind.RightParen, "Expected validation argument list end.");
            }

            document.Validations.Add(node);
            Match(TokenKind.Comma);
        }

        Consume(TokenKind.RightBrace, "Expected validate block end.");
    }

    private void ParseMetadataBlock(DocumentNode document)
    {
        Consume(TokenKind.LeftBrace, "Expected metadata block start.");

        while (!Check(TokenKind.RightBrace) && !IsAtEnd())
        {
            Token key = Consume(TokenKind.Identifier, "Expected metadata key.");
            Consume(TokenKind.Colon, "Expected ':' after metadata key.");
            Token value = ConsumeMetadataValue();
            document.Metadata[key.Value] = value.Value;
            Match(TokenKind.Comma);
        }

        Consume(TokenKind.RightBrace, "Expected metadata block end.");
    }

    private AstNode ParseExpression()
    {
        if (MatchIdentifier("project"))
        {
            AstNode source = ParseExpression();
            ConsumeIdentifier("as", "Expected 'as' in projection.");
            Token alias = Consume(TokenKind.Identifier, "Expected projection alias.");
            Consume(TokenKind.Arrow, "Expected projection arrow.");
            AstNode body = ParseExpression();

            return new ProjectionNode
            {
                SourceExpression = source,
                ItemAlias = alias.Value,
                BodyExpression = body
            };
        }

        return ParsePrimaryExpression();
    }

    private AstNode ParsePrimaryExpression()
    {
        if (Match(TokenKind.StringLiteral))
        {
            return new LiteralNode
            {
                DataType = "String",
                RawValue = Previous.Value
            };
        }

        if (Match(TokenKind.NumberLiteral))
        {
            return new LiteralNode
            {
                DataType = "Number",
                RawValue = Previous.Value
            };
        }

        if (Match(TokenKind.Path))
        {
            return new PathNode
            {
                Path = Previous.Value
            };
        }

        if (Match(TokenKind.Identifier))
        {
            Token identifier = Previous;

            if (string.Equals(identifier.Value, "true", StringComparison.Ordinal))
            {
                return new LiteralNode
                {
                    DataType = "Boolean",
                    RawValue = "true"
                };
            }

            if (string.Equals(identifier.Value, "false", StringComparison.Ordinal))
            {
                return new LiteralNode
                {
                    DataType = "Boolean",
                    RawValue = "false"
                };
            }

            if (string.Equals(identifier.Value, "null", StringComparison.Ordinal))
            {
                return new LiteralNode
                {
                    DataType = "Null",
                    RawValue = string.Empty,
                    IsNull = true
                };
            }

            if (Match(TokenKind.LeftParen))
            {
                return ParseFunctionCall(identifier);
            }

            return new PathNode
            {
                Path = identifier.Value
            };
        }

        if (Match(TokenKind.LeftBrace))
        {
            return ParseMapExpression();
        }

        if (Match(TokenKind.LeftBracket))
        {
            return ParseOrderedExpression();
        }

        throw Error(Current, "Expected expression.");
    }

    private AstNode ParseFunctionCall(Token identifier)
    {
        List<AstNode> arguments = [];
        ParseArguments(arguments);
        Consume(TokenKind.RightParen, "Expected function argument list end.");

        if (string.Equals(identifier.Value, "when", StringComparison.Ordinal))
        {
            if (arguments.Count != 3)
            {
                throw Error(identifier, "Conditional expression requires three arguments.");
            }

            return new ConditionNode
            {
                Condition = arguments[0],
                ThenExpression = arguments[1],
                ElseExpression = arguments[2]
            };
        }

        if (string.Equals(identifier.Value, "scalars", StringComparison.Ordinal))
        {
            ScalarCollectionNode collection = new();

            foreach (AstNode argument in arguments)
            {
                if (argument is not LiteralNode literal)
                {
                    throw Error(identifier, "Scalar collection literal accepts only scalar literal arguments.");
                }

                collection.Values.Add(literal);
            }

            return collection;
        }

        FunctionCallNode node = new()
        {
            FunctionKey = identifier.Value
        };

        node.Arguments.AddRange(arguments);
        return node;
    }

    private MapExpressionNode ParseMapExpression()
    {
        MapExpressionNode node = new();

        while (!Check(TokenKind.RightBrace) && !IsAtEnd())
        {
            Token name = Consume(TokenKind.Identifier, "Expected inline member name.");
            Consume(TokenKind.Colon, "Expected ':' after inline member name.");
            node.Properties.Add(new PropertyExpressionNode
            {
                Name = name.Value,
                Expression = ParseExpression()
            });
            Match(TokenKind.Comma);
        }

        Consume(TokenKind.RightBrace, "Expected inline expression end.");
        return node;
    }

    private OrderedExpressionNode ParseOrderedExpression()
    {
        OrderedExpressionNode node = new();

        while (!Check(TokenKind.RightBracket) && !IsAtEnd())
        {
            node.Items.Add(ParseExpression());
            Match(TokenKind.Comma);
        }

        Consume(TokenKind.RightBracket, "Expected ordered expression end.");
        return node;
    }

    private void ParseArguments(List<AstNode> arguments)
    {
        while (!Check(TokenKind.RightParen) && !IsAtEnd())
        {
            arguments.Add(ParseExpression());

            if (!Match(TokenKind.Comma))
            {
                break;
            }
        }
    }

    private Token ConsumeMetadataValue()
    {
        if (Check(TokenKind.StringLiteral) || Check(TokenKind.NumberLiteral) || Check(TokenKind.Identifier) || Check(TokenKind.Path))
        {
            return Advance();
        }

        throw Error(Current, "Expected metadata value.");
    }

    private Token ConsumeTargetPath(string message)
    {
        if (Check(TokenKind.Path) || Check(TokenKind.Identifier))
        {
            return Advance();
        }

        throw Error(Current, message);
    }

    private Token ConsumePathLike(string message)
    {
        if (Check(TokenKind.Path) || Check(TokenKind.Identifier))
        {
            return Advance();
        }

        throw Error(Current, message);
    }

    private void ConsumeIdentifier(string value, string message)
    {
        Token token = Consume(TokenKind.Identifier, message);

        if (!string.Equals(token.Value, value, StringComparison.Ordinal))
        {
            throw Error(token, message);
        }
    }

    private Token Consume(TokenKind kind, string message)
    {
        if (Check(kind))
        {
            return Advance();
        }

        throw Error(Current, message);
    }

    private bool MatchIdentifier(string value)
    {
        if (Check(TokenKind.Identifier) && string.Equals(Current.Value, value, StringComparison.Ordinal))
        {
            Advance();
            return true;
        }

        return false;
    }

    private bool Match(TokenKind kind)
    {
        if (Check(kind))
        {
            Advance();
            return true;
        }

        return false;
    }

    private bool Check(TokenKind kind)
    {
        return Current.Kind == kind;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            _position++;
        }

        return Previous;
    }

    private bool IsAtEnd()
    {
        return Current.Kind == TokenKind.End;
    }

    private Token Current => _tokens[_position];

    private Token Previous => _tokens[_position - 1];

    private static string CombinePath(string prefix, string name)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return name;
        }

        return $"{prefix}.{name}";
    }

    private static FormatException Error(Token token, string message)
    {
        return new FormatException($"{message} Line {token.Line}, column {token.Column}.");
    }
}
