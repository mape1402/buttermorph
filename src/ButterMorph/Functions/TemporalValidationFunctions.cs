namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Checks whether a value is a valid date.
/// </summary>
public sealed class IsDateFunction : IFunction
{
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether a value is a valid date.";

    private readonly FunctionTools _tools = new();

    private readonly DateFunctionTools _dates = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "isDate", 1, 1);
        return _tools.BooleanResult(_dates.TryParseDate(_tools.Text(_tools.Argument(context, "isDate", 0)), out _));
    }
}

/// <summary>
/// Checks whether a value is a valid date-time.
/// </summary>
public sealed class IsDateTimeFunction : IFunction
{
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether a value is a valid date-time.";

    private readonly FunctionTools _tools = new();

    private readonly DateFunctionTools _dates = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "isDateTime", 1, 1);
        return _tools.BooleanResult(_dates.TryParseDateTime(_tools.Text(_tools.Argument(context, "isDateTime", 0)), out _));
    }
}

/// <summary>
/// Checks whether a value is a valid time.
/// </summary>
public sealed class IsTimeFunction : IFunction
{
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether a value is a valid time.";

    private readonly FunctionTools _tools = new();

    private readonly DateFunctionTools _dates = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "isTime", 1, 1);
        return _tools.BooleanResult(_dates.TryParseTime(_tools.Text(_tools.Argument(context, "isTime", 0)), out _));
    }
}

/// <summary>
/// Checks whether a value is a valid duration.
/// </summary>
public sealed class IsTimeSpanFunction : IFunction
{
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether a value is a valid duration.";

    private readonly FunctionTools _tools = new();

    private readonly DateFunctionTools _dates = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "isTimeSpan", 1, 1);
        return _tools.BooleanResult(_dates.TryParseTimeSpan(_tools.Text(_tools.Argument(context, "isTimeSpan", 0)), out _));
    }
}

/// <summary>
/// Checks whether the first temporal value is before the second.
/// </summary>
public sealed class BeforeFunction : IFunction
{
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether the first temporal value is before the second.";

    private readonly TemporalComparisonTools _temporal = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        return _temporal.Compare(context, "before", comparison => comparison < 0);
    }
}

/// <summary>
/// Checks whether the first temporal value is after the second.
/// </summary>
public sealed class AfterFunction : IFunction
{
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether the first temporal value is after the second.";

    private readonly TemporalComparisonTools _temporal = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        return _temporal.Compare(context, "after", comparison => comparison > 0);
    }
}

/// <summary>
/// Checks whether the first temporal value is on or before the second.
/// </summary>
public sealed class OnOrBeforeFunction : IFunction
{
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether the first temporal value is on or before the second.";

    private readonly TemporalComparisonTools _temporal = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        return _temporal.Compare(context, "onOrBefore", comparison => comparison <= 0);
    }
}

/// <summary>
/// Checks whether the first temporal value is on or after the second.
/// </summary>
public sealed class OnOrAfterFunction : IFunction
{
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether the first temporal value is on or after the second.";

    private readonly TemporalComparisonTools _temporal = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        return _temporal.Compare(context, "onOrAfter", comparison => comparison >= 0);
    }
}

/// <summary>
/// Checks whether a temporal value is between two boundaries.
/// </summary>
public sealed class BetweenTemporalFunction : IFunction
{
    private readonly string _key;

    /// <summary>
    /// Initializes a new instance of the <see cref="BetweenTemporalFunction"/> class.
    /// </summary>
    /// <param name="key">The function key.</param>
    public BetweenTemporalFunction(string key)
    {
        _key = key;
    }

    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether a temporal value is between two boundaries.";

    private readonly TemporalComparisonTools _temporal = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        return _temporal.Between(context, _key);
    }
}

/// <summary>
/// Parses a time and returns a canonical time value.
/// </summary>
public sealed class ParseTimeFunction : IFunction
{
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Parses a time and returns a canonical time value.";

    private readonly FunctionTools _tools = new();

    private readonly DateFunctionTools _dates = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "parseTime", 1, 1);
        return _dates.TimeOnlyResult(_dates.ParseTimeOnly(_tools.Argument(context, "parseTime", 0)));
    }
}

/// <summary>
/// Parses a duration and returns a canonical duration value.
/// </summary>
public sealed class ParseTimeSpanFunction : IFunction
{
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Parses a duration and returns a canonical duration value.";

    private readonly FunctionTools _tools = new();

    private readonly DateFunctionTools _dates = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "parseTimeSpan", 1, 1);
        return _dates.TimeSpanResult(_dates.ParseDuration(_tools.Argument(context, "parseTimeSpan", 0)));
    }
}

internal sealed class TemporalComparisonTools
{
    private readonly FunctionTools _tools = new();

    private readonly DateFunctionTools _dates = new();

    internal IFunctionResult Compare(FunctionExecutionContext context, string key, Func<int, bool> predicate)
    {
        _tools.Require(context, key, 2, 2);
        return _tools.BooleanResult(predicate(CompareValues(
            _tools.Text(_tools.Argument(context, key, 0)),
            _tools.Text(_tools.Argument(context, key, 1)))));
    }

    internal IFunctionResult Between(FunctionExecutionContext context, string key)
    {
        _tools.Require(context, key, 3, 3);
        string value = _tools.Text(_tools.Argument(context, key, 0));
        string minimum = _tools.Text(_tools.Argument(context, key, 1));
        string maximum = _tools.Text(_tools.Argument(context, key, 2));
        return _tools.BooleanResult(CompareValues(value, minimum) >= 0 && CompareValues(value, maximum) <= 0);
    }

    private int CompareValues(string left, string right)
    {
        if (_dates.TryParseTimeSpan(left, out TimeSpan leftDuration) && _dates.TryParseTimeSpan(right, out TimeSpan rightDuration))
        {
            return leftDuration.CompareTo(rightDuration);
        }

        if (_dates.TryParseDate(left, out DateOnly leftDate) && _dates.TryParseDate(right, out DateOnly rightDate))
        {
            return leftDate.CompareTo(rightDate);
        }

        if (_dates.TryParseTime(left, out TimeOnly leftTime) && _dates.TryParseTime(right, out TimeOnly rightTime))
        {
            return leftTime.CompareTo(rightTime);
        }

        if (_dates.TryParseDateTime(left, out DateTimeOffset leftDateTime) && _dates.TryParseDateTime(right, out DateTimeOffset rightDateTime))
        {
            return leftDateTime.CompareTo(rightDateTime);
        }

        throw new InvalidOperationException($"Values '{left}' and '{right}' are not comparable temporal values.");
    }
}
