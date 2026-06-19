namespace ButterMorph.Functions;

using System.Globalization;
using ButterMorph.Abstractions;

internal sealed class DateFunctionTools
{
    // Shared scalar conversion helper.
    private readonly FunctionTools _tools = new();

    internal DateTimeOffset Parse(IFunctionArgument argument)
    {
        string text = _tools.Text(argument);

        if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset offset))
        {
            return offset;
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dateTime))
        {
            return new DateTimeOffset(dateTime);
        }

        throw new InvalidOperationException($"Value '{text}' is not a valid date.");
    }

    internal DateTimeOffset ParseExact(IFunctionArgument argument, string format)
    {
        string text = _tools.Text(argument);

        if (DateTimeOffset.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset offset))
        {
            return offset;
        }

        if (DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dateTime))
        {
            return new DateTimeOffset(dateTime);
        }

        throw new InvalidOperationException($"Value '{text}' does not match date format '{format}'.");
    }

    internal IFunctionResult DateResult(DateTimeOffset value)
    {
        return _tools.StringResult(value.ToString("O", CultureInfo.InvariantCulture));
    }

    internal IFunctionResult DateOnlyResult(DateTimeOffset value)
    {
        return _tools.StringResult(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}
