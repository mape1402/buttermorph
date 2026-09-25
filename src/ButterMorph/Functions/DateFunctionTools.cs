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

        if (TryParseDateTime(text, out DateTimeOffset offset))
        {
            return offset;
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

    internal DateOnly ParseDateOnly(IFunctionArgument argument)
    {
        string text = _tools.Text(argument);

        if (TryParseDate(text, out DateOnly date))
        {
            return date;
        }

        throw new InvalidOperationException($"Value '{text}' is not a valid date.");
    }

    internal TimeOnly ParseTimeOnly(IFunctionArgument argument)
    {
        string text = _tools.Text(argument);

        if (TryParseTime(text, out TimeOnly time))
        {
            return time;
        }

        throw new InvalidOperationException($"Value '{text}' is not a valid time.");
    }

    internal TimeSpan ParseDuration(IFunctionArgument argument)
    {
        string text = _tools.Text(argument);

        if (TryParseTimeSpan(text, out TimeSpan duration))
        {
            return duration;
        }

        throw new InvalidOperationException($"Value '{text}' is not a valid duration.");
    }

    internal IFunctionResult DateResult(DateTimeOffset value)
    {
        return _tools.ScalarResult(new ButterMorph.Core.ScalarValue
        {
            DataType = "DateTime",
            RawValue = value.ToString("O", CultureInfo.InvariantCulture),
            IsNull = false
        });
    }

    internal IFunctionResult DateOnlyResult(DateTimeOffset value)
    {
        return DateOnlyResult(DateOnly.FromDateTime(value.DateTime));
    }

    internal IFunctionResult DateOnlyResult(DateOnly value)
    {
        return _tools.ScalarResult(new ButterMorph.Core.ScalarValue
        {
            DataType = "Date",
            RawValue = value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            IsNull = false
        });
    }

    internal IFunctionResult TimeOnlyResult(TimeOnly value)
    {
        return _tools.ScalarResult(new ButterMorph.Core.ScalarValue
        {
            DataType = "Time",
            RawValue = value.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            IsNull = false
        });
    }

    internal IFunctionResult TimeSpanResult(TimeSpan value)
    {
        return _tools.ScalarResult(new ButterMorph.Core.ScalarValue
        {
            DataType = "TimeSpan",
            RawValue = value.ToString("c", CultureInfo.InvariantCulture),
            IsNull = false
        });
    }

    internal bool TryParseDateTime(string text, out DateTimeOffset value)
    {
        return TryParseDateTimeValue(text, out value);
    }

    internal bool TryParseDate(string text, out DateOnly value)
    {
        return DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out value) ||
            DateOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
    }

    internal bool TryParseTime(string text, out TimeOnly value)
    {
        return TimeOnly.TryParseExact(text, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out value) ||
            TimeOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
    }

    internal bool TryParseTimeSpan(string text, out TimeSpan value)
    {
        if (TimeSpan.TryParseExact(text, "c", CultureInfo.InvariantCulture, out value) ||
            TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        try
        {
            value = System.Xml.XmlConvert.ToTimeSpan(text);
            return true;
        }
        catch (Exception ex) when (ex is FormatException or OverflowException)
        {
            value = default;
            return false;
        }
    }

    private static bool TryParseDateTimeValue(string text, out DateTimeOffset offset)
    {
        if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out offset))
        {
            return true;
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dateTime))
        {
            offset = new DateTimeOffset(dateTime);
            return true;
        }

        return false;
    }
}
