namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

internal sealed class NativeFunctionRegistration
{
    // Creates descriptors for native function discovery.
    private readonly FunctionDescriptorFactory _descriptors = new();

    internal void Register(IFunctionRegistry registry)
    {
        Register(registry, "concat", new ConcatFunction(), FunctionValueKind.Scalar, "Text", 0, 16);
        Register(registry, "upper", new UpperFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "lower", new LowerFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "trim", new TrimFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "replace", new ReplaceFunction(), FunctionValueKind.Scalar, "Text", 3, 3);
        Register(registry, "substring", new SubstringFunction(), FunctionValueKind.Scalar, "Text", 2, 3);
        Register(registry, "startsWith", new StartsWithFunction(), FunctionValueKind.Scalar, "Text", 2, 2);
        Register(registry, "endsWith", new EndsWithFunction(), FunctionValueKind.Scalar, "Text", 2, 2);
        Register(registry, "contains", new ContainsFunction(), FunctionValueKind.Scalar, "Text", 2, 2);
        Register(registry, "split", new SplitFunction(), FunctionValueKind.ScalarCollection, "Text", 2, 2);
        Register(registry, "splitLines", new SplitLinesFunction(), FunctionValueKind.ScalarCollection, "Text", 1, 1);
        Register(registry, "length", new LengthFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "toString", new ToStringFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "toNumber", new ToNumberFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "toBoolean", new ToBooleanFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "numberFormat", new NumberFormatFunction(), FunctionValueKind.Scalar, "Text", 2, 2);
        Register(registry, "trimStart", new TrimStartFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "trimEnd", new TrimEndFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "left", new LeftFunction(), FunctionValueKind.Scalar, "Text", 2, 2);
        Register(registry, "right", new RightFunction(), FunctionValueKind.Scalar, "Text", 2, 2);
        Register(registry, "indexOf", new IndexOfFunction(), FunctionValueKind.Scalar, "Text", 2, 2);
        Register(registry, "lastIndexOf", new LastIndexOfFunction(), FunctionValueKind.Scalar, "Text", 2, 2);
        Register(registry, "padLeft", new PadLeftFunction(), FunctionValueKind.Scalar, "Text", 3, 3);
        Register(registry, "padRight", new PadRightFunction(), FunctionValueKind.Scalar, "Text", 3, 3);
        Register(registry, "capitalize", new CapitalizeFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "camelCase", new CamelCaseFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "normalizeWhitespace", new NormalizeWhitespaceFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "ToUpper", new ToUpperFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "ToLower", new ToLowerFunction(), FunctionValueKind.Scalar, "Text", 1, 1);
        Register(registry, "default", new DefaultFunction(), FunctionValueKind.Scalar, "Fallback", 2, 2);
        Register(registry, "defaultEmpty", new DefaultEmptyFunction(), FunctionValueKind.Scalar, "Fallback", 2, 2);
        Register(registry, "coalesce", new CoalesceFunction(), FunctionValueKind.Scalar, "Fallback", 1, 16);
        Register(registry, "exists", new ExistsFunction(), FunctionValueKind.Scalar, "Null", 1, 1);
        Register(registry, "isNull", new IsNullFunction(), FunctionValueKind.Scalar, "Null", 1, 1);
        Register(registry, "isEmpty", new IsEmptyFunction(), FunctionValueKind.Scalar, "Null", 1, 1);
        Register(registry, "eq", new EqualToFunction(), FunctionValueKind.Scalar, "Logic", 2, 2);
        Register(registry, "neq", new NotEqualToFunction(), FunctionValueKind.Scalar, "Logic", 2, 2);
        Register(registry, "gt", new GreaterThanFunction(), FunctionValueKind.Scalar, "Logic", 2, 2);
        Register(registry, "gte", new GreaterOrEqualFunction(), FunctionValueKind.Scalar, "Logic", 2, 2);
        Register(registry, "lt", new LessThanFunction(), FunctionValueKind.Scalar, "Logic", 2, 2);
        Register(registry, "lte", new LessOrEqualFunction(), FunctionValueKind.Scalar, "Logic", 2, 2);
        Register(registry, "and", new AndFunction(), FunctionValueKind.Scalar, "Logic", 0, 16);
        Register(registry, "or", new OrFunction(), FunctionValueKind.Scalar, "Logic", 0, 16);
        Register(registry, "not", new NotFunction(), FunctionValueKind.Scalar, "Logic", 1, 1);
        Register(registry, "if", new IfFunction(), FunctionValueKind.Scalar, "Control", 3, 3);
        Register(registry, "switch", new SwitchFunction(), FunctionValueKind.Scalar, "Control", 3, 17);
        Register(registry, "try", new TryFunction(), FunctionValueKind.Scalar, "Control", 1, 2);
        Register(registry, "assert", new AssertFunction(), FunctionValueKind.Scalar, "Control", 1, 2);
        Register(registry, "add", new AddFunction(), FunctionValueKind.Scalar, "Math", 1, 16);
        Register(registry, "sub", new SubFunction(), FunctionValueKind.Scalar, "Math", 1, 16);
        Register(registry, "mul", new MulFunction(), FunctionValueKind.Scalar, "Math", 1, 16);
        Register(registry, "div", new DivFunction(), FunctionValueKind.Scalar, "Math", 2, 16);
        Register(registry, "mod", new ModFunction(), FunctionValueKind.Scalar, "Math", 2, 2);
        Register(registry, "abs", new AbsFunction(), FunctionValueKind.Scalar, "Math", 1, 1);
        Register(registry, "round", new RoundFunction(), FunctionValueKind.Scalar, "Math", 1, 1);
        Register(registry, "floor", new FloorFunction(), FunctionValueKind.Scalar, "Math", 1, 1);
        Register(registry, "ceil", new CeilFunction(), FunctionValueKind.Scalar, "Math", 1, 1);
        Register(registry, "min", new MinFunction(), FunctionValueKind.Scalar, "Math", 1, 16);
        Register(registry, "max", new MaxFunction(), FunctionValueKind.Scalar, "Math", 1, 16);
        Register(registry, "toArray", new ToArrayFunction(), FunctionValueKind.ScalarCollection, "Collections", 1, 1);
        Register(registry, "first", new FirstFunction(), FunctionValueKind.Scalar, "Collections", 1, 1);
        Register(registry, "last", new LastFunction(), FunctionValueKind.Scalar, "Collections", 1, 1);
        Register(registry, "count", new CountFunction(), FunctionValueKind.Scalar, "Collections", 1, 1);
        Register(registry, "join", new JoinFunction(), FunctionValueKind.Scalar, "Collections", 2, 2);
        Register(registry, "filter", new FilterFunction(), FunctionValueKind.ScalarCollection, "Collections", 2, 2);
        Register(registry, "map", new MapFunction(), FunctionValueKind.ScalarCollection, "Collections", 1, 1);
        Register(registry, "reduce", new ReduceFunction(), FunctionValueKind.Scalar, "Collections", 1, 2);
        Register(registry, "sort", new SortFunction(), FunctionValueKind.ScalarCollection, "Collections", 1, 2);
        Register(registry, "distinct", new DistinctFunction(), FunctionValueKind.ScalarCollection, "Collections", 1, 2);
        Register(registry, "groupBy", new GroupByFunction(), FunctionValueKind.StructureNode, "Collections", 2, 2);
        Register(registry, "flatten", new FlattenFunction(), FunctionValueKind.ScalarCollection, "Collections", 1, 2);
        Register(registry, "zip", new ZipFunction(), FunctionValueKind.StructureNodeCollection, "Collections", 2, 2);
        Register(registry, "take", new TakeFunction(), FunctionValueKind.ScalarCollection, "Collections", 2, 2);
        Register(registry, "skip", new SkipFunction(), FunctionValueKind.ScalarCollection, "Collections", 2, 2);
        Register(registry, "slice", new SliceFunction(), FunctionValueKind.ScalarCollection, "Collections", 3, 3);
        Register(registry, "reverse", new ReverseFunction(), FunctionValueKind.ScalarCollection, "Collections", 1, 1);
        Register(registry, "sum", new SumFunction(), FunctionValueKind.Scalar, "Collections", 1, 1);
        Register(registry, "average", new AverageFunction(), FunctionValueKind.Scalar, "Collections", 1, 1);
        Register(registry, "any", new AnyFunction(), FunctionValueKind.Scalar, "Collections", 1, 1);
        Register(registry, "all", new AllFunction(), FunctionValueKind.Scalar, "Collections", 1, 1);
        Register(registry, "containsValue", new ContainsValueFunction(), FunctionValueKind.Scalar, "Collections", 2, 2);
        Register(registry, "today", new TodayFunction(), FunctionValueKind.Scalar, "Date", 0, 0);
        Register(registry, "now", new NowFunction(), FunctionValueKind.Scalar, "Date", 0, 0);
        Register(registry, "todayLocal", new TodayLocalFunction(), FunctionValueKind.Scalar, "Date", 0, 0);
        Register(registry, "nowLocal", new NowLocalFunction(), FunctionValueKind.Scalar, "Date", 0, 0);
        Register(registry, "dateAddDays", new AddDaysFunction(), FunctionValueKind.Scalar, "Date", 2, 2);
        Register(registry, "dateAddHours", new DateAddHoursFunction(), FunctionValueKind.Scalar, "Date", 2, 2);
        Register(registry, "dateAddMinutes", new DateAddMinutesFunction(), FunctionValueKind.Scalar, "Date", 2, 2);
        Register(registry, "dateAddMonths", new DateAddMonthsFunction(), FunctionValueKind.Scalar, "Date", 2, 2);
        Register(registry, "dateAddYears", new DateAddYearsFunction(), FunctionValueKind.Scalar, "Date", 2, 2);
        Register(registry, "diffDays", new DiffDaysFunction(), FunctionValueKind.Scalar, "Date", 2, 2);
        Register(registry, "diffHours", new DiffHoursFunction(), FunctionValueKind.Scalar, "Date", 2, 2);
        Register(registry, "diffMinutes", new DiffMinutesFunction(), FunctionValueKind.Scalar, "Date", 2, 2);
        Register(registry, "formatDate", new FormatDateFunction(), FunctionValueKind.Scalar, "Date", 2, 2);
        Register(registry, "parseDate", new ParseDateFunction(), FunctionValueKind.Scalar, "Date", 1, 2);
        Register(registry, "toTimeZone", new ToTimeZoneFunction(), FunctionValueKind.Scalar, "Date", 2, 2);
        Register(registry, "year", new YearFunction(), FunctionValueKind.Scalar, "Date", 1, 1);
        Register(registry, "month", new MonthFunction(), FunctionValueKind.Scalar, "Date", 1, 1);
        Register(registry, "day", new DayFunction(), FunctionValueKind.Scalar, "Date", 1, 1);
        Register(registry, "startOfMonth", new StartOfMonthFunction(), FunctionValueKind.Scalar, "Date", 1, 1);
        Register(registry, "endOfMonth", new EndOfMonthFunction(), FunctionValueKind.Scalar, "Date", 1, 1);
        Register(registry, "regexMatch", new RegexMatchFunction(), FunctionValueKind.Scalar, "Regex", 2, 3);
        Register(registry, "regexExtract", new RegexExtractFunction(), FunctionValueKind.Scalar, "Regex", 2, 4);
        Register(registry, "regexReplace", new RegexReplaceFunction(), FunctionValueKind.Scalar, "Regex", 3, 4);
        Register(registry, "regexSplit", new RegexSplitFunction(), FunctionValueKind.ScalarCollection, "Regex", 2, 3);
        Register(registry, "regexFindAll", new RegexFindAllFunction(), FunctionValueKind.ScalarCollection, "Regex", 2, 4);
        Register(registry, "jsonParse", new JsonParseFunction(), FunctionValueKind.StructureNode, "Json", 1, 1);
        Register(registry, "jsonStringify", new JsonStringifyFunction(), FunctionValueKind.Scalar, "Json", 1, 1);
        Register(registry, "uuid", new UuidFunction(), FunctionValueKind.Scalar, "Ids", 0, 0);
        Register(registry, "ulid", new UlidFunction(), FunctionValueKind.Scalar, "Ids", 0, 0);
        Register(registry, "hash", new HashFunction(), FunctionValueKind.Scalar, "Hash", 2, 2);
    }

    // Registers one native function with its descriptor.
    private void Register(IFunctionRegistry registry, string key, IFunction function, FunctionValueKind valueKind, string category, int minimum, int maximum)
    {
        registry.Register(key, function, _descriptors.Create(key, function, valueKind, category, minimum, maximum));
    }
}
