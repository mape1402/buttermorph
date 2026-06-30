namespace ButterMorph.Functions;

using System.Globalization;
using System.Text;
using ButterMorph.Abstractions;

/// <summary>
/// Converts text to camel case.
/// </summary>
public sealed class CamelCaseFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Converts text to camel case.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "camelCase", 1, 1);
        return _tools.StringResult(ConvertText(_tools.Text(context.Arguments[0])));
    }

    // Converts separated words into camel case text.
    private static string ConvertText(string text)
    {
        List<string> words = ReadWords(text);

        if (words.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        builder.Append(words[0].ToLower(CultureInfo.InvariantCulture));

        for (int index = 1; index < words.Count; index++)
        {
            string word = words[index].ToLower(CultureInfo.InvariantCulture);
            builder.Append(char.ToUpper(word[0], CultureInfo.InvariantCulture));
            builder.Append(word[1..]);
        }

        return builder.ToString();
    }

    // Reads alphanumeric word segments from text.
    private static List<string> ReadWords(string text)
    {
        List<string> words = [];
        StringBuilder current = new();

        foreach (char character in text)
        {
            if (char.IsLetterOrDigit(character))
            {
                current.Append(character);
                continue;
            }

            AddCurrentWord(words, current);
        }

        AddCurrentWord(words, current);
        return words;
    }

    // Adds the current word buffer when it has content.
    private static void AddCurrentWord(List<string> words, StringBuilder current)
    {
        if (current.Length == 0)
        {
            return;
        }

        words.Add(current.ToString());
        current.Clear();
    }
}
