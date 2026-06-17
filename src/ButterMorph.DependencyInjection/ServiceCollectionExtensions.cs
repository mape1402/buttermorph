namespace ButterMorph.DependencyInjection;

using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Dsl;
using ButterMorph.Execution;
using ButterMorph.Functions;
using ButterMorph.Navigation;
using ButterMorph.Semantics;
using ButterMorph.Transformation;
using ButterMorph.Validation;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides service registration helpers for ButterMorph.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the default ButterMorph service registrations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddButterMorph(this IServiceCollection services)
    {
        services.AddTransient<IButterMorphEngine, global::ButterMorph.ButterMorphEngine>();
        services.AddTransient<IDslParser, DslParser>();
        services.AddTransient<IDslExporter, DslExporter>();
        services.AddTransient<IExecutionPipeline, ExecutionPipeline>();
        services.AddTransient<IExecutionContextFactory, ExecutionContextFactory>();
        services.AddTransient<INavigationEngine, NavigationEngine>();
        services.AddTransient<IPathResolver, PathResolver>();
        services.AddTransient<ISchemaPathResolver, SchemaPathResolver>();
        services.AddTransient<ITransformationSemanticAnalyzer, TransformationSemanticAnalyzer>();
        services.AddSingleton<IFunctionRegistry, FunctionRegistry>();
        services.AddTransient<ITransformationExpressionEvaluator, TransformationExpressionEvaluator>();
        services.AddTransient<ITransformationEngine, TransformationEngine>();
        services.AddTransient<IValidationEngine, ValidationEngine>();
        services.AddSingleton<IValidationRuleRegistry, ValidationRuleRegistry>();

        return services;
    }
}
