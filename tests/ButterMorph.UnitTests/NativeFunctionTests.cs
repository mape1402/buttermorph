namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.DependencyInjection;
using ButterMorph.Functions;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Verifies native ButterMorph function behavior.
/// </summary>
public sealed class NativeFunctionTests
{
    /// <summary>
    /// Confirms that all inventoried native functions are registered by dependency injection.
    /// </summary>
    [Fact]
    public void AddButterMorphRegistersNativeFunctionInventory()
    {
        using ServiceProvider provider = CreateProvider();
        IFunctionRegistry registry = provider.GetRequiredService<IFunctionRegistry>();
        List<string> missing = [];

        foreach (string key in Inventory())
        {
            try
            {
                registry.Resolve(key);
                registry.ResolveDescriptor(key);
            }
            catch (KeyNotFoundException)
            {
                missing.Add(key);
            }
        }

        Assert.Empty(missing);
        Assert.Equal(Inventory().Count, registry.ListDescriptors().Count);
    }

    /// <summary>
    /// Confirms text, fallback, and null native functions.
    /// </summary>
    [Fact]
    public void TextFallbackAndNullFunctionsWork()
    {
        AssertScalar("Hello WORLD", new ConcatFunction().Execute(Context(Text("Hello "), Text("WORLD"))));
        AssertScalar("HELLO", new UpperFunction().Execute(Context(Text("hello"))));
        AssertScalar("ell", new SubstringFunction().Execute(Context(Text("hello"), Number("1"), Number("3"))));
        AssertScalar("fallback", new DefaultFunction().Execute(Context(Null(), Text("fallback"))));
        AssertScalar("fallback", new CoalesceFunction().Execute(Context(Null(), Text("fallback"))));
        AssertScalar("true", new IsEmptyFunction().Execute(Context(Text(" "))));
    }

    /// <summary>
    /// Confirms logic, control, and math native functions.
    /// </summary>
    [Fact]
    public void LogicControlAndMathFunctionsWork()
    {
        AssertScalar("true", new EqualToFunction().Execute(Context(Number("10"), Number("10"))));
        AssertScalar("true", new GreaterThanFunction().Execute(Context(Number("11"), Number("10"))));
        AssertScalar("false", new AndFunction().Execute(Context(Boolean("true"), Boolean("false"))));
        AssertScalar("yes", new IfFunction().Execute(Context(Boolean("true"), Text("yes"), Text("no"))));
        AssertScalar("matched", new SwitchFunction().Execute(Context(Text("b"), Text("a"), Text("first"), Text("b"), Text("matched"), Text("fallback"))));
        AssertScalar("fallback", new TryFunction().Execute(Context(Text(""), Text("fallback"))));
        AssertScalar("15", new AddFunction().Execute(Context(Number("10"), Number("5"))));
        AssertScalar("5", new SubFunction().Execute(Context(Number("10"), Number("5"))));
        AssertScalar("8", new DivFunction().Execute(Context(Number("16"), Number("2"))));
        AssertScalar("2", new RoundFunction().Execute(Context(Number("1.6"))));
    }

    /// <summary>
    /// Confirms collection native functions.
    /// </summary>
    [Fact]
    public void CollectionFunctionsWork()
    {
        IFunctionArgument values = Scalars("b", "a", "b");
        IFunctionArgument masks = ScalarValues(BooleanValue("true"), BooleanValue("false"), BooleanValue("true"));

        AssertScalar("b", new FirstFunction().Execute(Context(values)));
        AssertScalar("3", new CountFunction().Execute(Context(values)));
        AssertScalar("b|a|b", new JoinFunction().Execute(Context(values, Text("|"))));
        AssertCollection(["b", "b"], new FilterFunction().Execute(Context(values, masks)));
        AssertCollection(["a", "b", "b"], new SortFunction().Execute(Context(values)));
        AssertCollection(["b", "a"], new DistinctFunction().Execute(Context(values)));
        AssertScalar("bab", new ReduceFunction().Execute(Context(values)));
        Assert.NotNull(new GroupByFunction().Execute(Context(values, Scalars("x", "y", "x"))));
        Assert.NotNull(new ZipFunction().Execute(Context(Scalars("1", "2"), Scalars("a", "b"))));
    }

    /// <summary>
    /// Confirms date, regex, JSON, id, and hash native functions.
    /// </summary>
    [Fact]
    public void DateRegexJsonIdAndHashFunctionsWork()
    {
        AssertScalar("2024-01-03T00:00:00.0000000+00:00", new AddDaysFunction().Execute(Context(Text("2024-01-01T00:00:00Z"), Number("2"))));
        AssertScalar("2", new DiffDaysFunction().Execute(Context(Text("2024-01-03T00:00:00Z"), Text("2024-01-01T00:00:00Z"))));
        AssertScalar("2024", new FormatDateFunction().Execute(Context(Text("2024-01-01T00:00:00Z"), Text("yyyy"))));
        AssertScalar("true", new RegexMatchFunction().Execute(Context(Text("abc123"), Text("[0-9]+"))));
        AssertScalar("123", new RegexExtractFunction().Execute(Context(Text("abc123"), Text("[0-9]+"))));
        AssertCollection(["a", "b"], new RegexSplitFunction().Execute(Context(Text("a,b"), Text(","))));
        IFunctionResult node = new JsonParseFunction().Execute(Context(Text("{\"name\":\"Ada\"}")));
        Assert.IsAssignableFrom<IStructureNodeFunctionResult>(node);
        AssertScalar("{\"name\":\"Ada\"}", new JsonStringifyFunction().Execute(Context(new StructureNodeFunctionArgument
        {
            Node = ((IStructureNodeFunctionResult)node).Node
        })));
        Assert.False(string.IsNullOrWhiteSpace(ScalarText(new UuidFunction().Execute(Context()))));
        Assert.False(string.IsNullOrWhiteSpace(ScalarText(new UlidFunction().Execute(Context()))));
        Assert.Equal(64, ScalarText(new HashFunction().Execute(Context(Text("sha256"), Text("abc")))).Length);
    }

    /// <summary>
    /// Confirms native functions execute inside transformation mappings.
    /// </summary>
    [Fact]
    public void TransformationEngineUsesNativeFunctionRegistration()
    {
        using ServiceProvider provider = CreateProvider();
        ITransformationEngine engine = provider.GetRequiredService<ITransformationEngine>();
        ITransformationDocument document = new TransformationDocument
        {
            Mappings =
            [
                new TransformationMapping
                {
                    TargetPath = "Greeting",
                    SourceExpression = new FunctionCallExpression
                    {
                        FunctionKey = "concat",
                        Arguments =
                        [
                            new ScalarLiteralExpression
                            {
                                Value = StringValue("Hello")
                            },
                            new ScalarLiteralExpression
                            {
                                Value = StringValue(" ButterMorph")
                            }
                        ]
                    }
                }
            ]
        };

        TransformationResult result = engine.Transform(new TransformationRequest
        {
            Definition = document,
            Sources = new Dictionary<string, IStructureGraph>()
        });

        IScalarStructureNode greeting = Assert.IsAssignableFrom<IScalarStructureNode>(result.ResultGraph.Root.Children.Single());
        Assert.Equal("Hello ButterMorph", greeting.Value.RawValue);
    }

    // Creates the dependency injection provider used by native function tests.
    private static ServiceProvider CreateProvider()
    {
        ServiceCollection services = new();
        services.AddButterMorph();
        return services.BuildServiceProvider();
    }

    // Creates a function execution context.
    private static FunctionExecutionContext Context(params IFunctionArgument[] arguments)
    {
        return new FunctionExecutionContext
        {
            ExecutionContext = new ExecutionContext(),
            Arguments = arguments
        };
    }

    // Creates a scalar text argument.
    private static IFunctionArgument Text(string value)
    {
        return new ScalarFunctionArgument
        {
            Value = StringValue(value)
        };
    }

    // Creates a scalar number argument.
    private static IFunctionArgument Number(string value)
    {
        return new ScalarFunctionArgument
        {
            Value = new ScalarValue
            {
                DataType = "Number",
                RawValue = value,
                IsNull = false
            }
        };
    }

    // Creates a scalar boolean argument.
    private static IFunctionArgument Boolean(string value)
    {
        return new ScalarFunctionArgument
        {
            Value = BooleanValue(value)
        };
    }

    // Creates a null scalar argument.
    private static IFunctionArgument Null()
    {
        return new ScalarFunctionArgument
        {
            Value = new ScalarValue
            {
                DataType = "Null",
                RawValue = string.Empty,
                IsNull = true
            }
        };
    }

    // Creates a scalar value collection argument.
    private static IFunctionArgument Scalars(params string[] values)
    {
        return new ScalarCollectionFunctionArgument
        {
            Values = values.Select(StringValue).ToList()
        };
    }

    // Creates a scalar value collection argument from values.
    private static IFunctionArgument ScalarValues(params IScalarValue[] values)
    {
        return new ScalarCollectionFunctionArgument
        {
            Values = values.ToList()
        };
    }

    // Creates a string scalar value.
    private static IScalarValue StringValue(string value)
    {
        return new ScalarValue
        {
            DataType = "String",
            RawValue = value,
            IsNull = false
        };
    }

    // Creates a boolean scalar value.
    private static IScalarValue BooleanValue(string value)
    {
        return new ScalarValue
        {
            DataType = "Boolean",
            RawValue = value,
            IsNull = false
        };
    }

    // Asserts a scalar result raw value.
    private static void AssertScalar(string expected, IFunctionResult result)
    {
        Assert.Equal(expected, ScalarText(result));
    }

    // Reads a scalar result raw value.
    private static string ScalarText(IFunctionResult result)
    {
        IScalarFunctionResult scalar = Assert.IsAssignableFrom<IScalarFunctionResult>(result);
        return scalar.Value.RawValue;
    }

    // Asserts scalar collection raw values.
    private static void AssertCollection(IReadOnlyCollection<string> expected, IFunctionResult result)
    {
        IScalarCollectionFunctionResult collection = Assert.IsAssignableFrom<IScalarCollectionFunctionResult>(result);
        Assert.Equal(expected, collection.Values.Select(value => value.RawValue).ToList());
    }

    // Provides the native function inventory.
    private static IReadOnlyCollection<string> Inventory()
    {
        return
        [
            "concat", "upper", "lower", "trim", "replace", "substring", "startsWith", "endsWith", "contains", "default", "defaultEmpty", "coalesce", "exists", "isNull", "isEmpty",
            "eq", "neq", "gt", "gte", "lt", "lte", "and", "or", "not", "if", "switch", "try", "assert",
            "add", "sub", "mul", "div", "mod", "abs", "round", "floor", "ceil", "min", "max",
            "toArray", "first", "last", "count", "join", "filter", "map", "reduce", "sort", "distinct", "groupBy", "flatten", "zip",
            "today", "now", "todayLocal", "nowLocal", "dateAddDays", "dateAddMonths", "dateAddYears", "diffDays", "diffHours", "diffMinutes", "formatDate", "parseDate", "toTimeZone", "year", "month", "day", "startOfMonth", "endOfMonth",
            "regexMatch", "regexExtract", "regexReplace", "regexSplit", "regexFindAll", "jsonParse", "jsonStringify", "uuid", "ulid", "hash"
        ];
    }
}
