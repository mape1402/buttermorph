namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Verifies transformation expression model containers.
/// </summary>
public sealed class TransformationExpressionModelTests
{
    /// <summary>
    /// Confirms that function call expressions preserve key and arguments.
    /// </summary>
    [Fact]
    public void FunctionCallExpressionPreservesKeyAndArguments()
    {
        PathExpression argument = new()
        {
            Path = "$source.Name"
        };
        FunctionCallExpression expression = new()
        {
            FunctionKey = "normalize",
            Arguments =
            [
                argument
            ]
        };

        ITransformationExpression storedArgument = Assert.Single(expression.Arguments);

        Assert.Equal(TransformationExpressionKind.FunctionCall, expression.Kind);
        Assert.Equal("normalize", expression.FunctionKey);
        Assert.Same(argument, storedArgument);
    }
}
