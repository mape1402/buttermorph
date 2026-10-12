namespace ButterMorph.Web.Razor;

using System.Reflection;
using ButterMorph.Contracts;

/// <summary>
/// Discovers ButterMorph language type bundles through reflection.
/// </summary>
public sealed class ReflectionButterMorphLanguageTypeBundleDiscovery : IButterMorphLanguageTypeBundleDiscovery
{
    /// <inheritdoc />
    public ButterMorphLanguageTypeBundleDiscoveryResult Discover(Stream assemblyStream, string fileName)
    {
        if (assemblyStream == null || assemblyStream == Stream.Null)
        {
            return Failed("Assembly file is required.");
        }

        string extension = Path.GetExtension(fileName);
        string path = Path.Combine(Path.GetTempPath(), "buttermorph-" + Guid.NewGuid().ToString("N") + extension);
        try
        {
            using (FileStream file = File.Create(path))
            {
                assemblyStream.CopyTo(file);
            }

            Assembly assembly = Assembly.LoadFrom(path);
            return Discover(assembly);
        }
        catch (Exception exception) when (exception is BadImageFormatException or FileLoadException or FileNotFoundException or ReflectionTypeLoadException)
        {
            return Failed(exception.Message);
        }
        finally
        {
            TryDelete(path);
        }
    }

    /// <inheritdoc />
    public ButterMorphLanguageTypeBundleDiscoveryResult Discover(Assembly assembly)
    {
        if (assembly == null)
        {
            return Failed("Assembly is required.");
        }

        List<string> diagnostics = [];
        Type[] types = GetLoadableTypes(assembly, diagnostics);
        Type providerType = types.FirstOrDefault(type =>
            typeof(IButterMorphLanguageTypeBundle).IsAssignableFrom(type) &&
            type is { IsAbstract: false, IsInterface: false } &&
            type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null);

        if (providerType == null)
        {
            return Failed("Assembly does not expose an IButterMorphLanguageTypeBundle implementation.");
        }

        IButterMorphLanguageTypeBundle provider = (IButterMorphLanguageTypeBundle)Activator.CreateInstance(providerType, true);
        string bundleKey = provider.Key ?? string.Empty;
        string language = provider.Language ?? string.Empty;
        if (string.IsNullOrWhiteSpace(bundleKey))
        {
            diagnostics.Add("Bundle key is required.");
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            diagnostics.Add("Bundle language is required.");
        }

        List<ButterMorphLanguageType> discoveredTypes = [];
        foreach (Type type in types)
        {
            ButterMorphLanguageTypeAttribute attribute = type
                .GetCustomAttributes(typeof(ButterMorphLanguageTypeAttribute), false)
                .OfType<ButterMorphLanguageTypeAttribute>()
                .FirstOrDefault();
            if (attribute == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(attribute.Key))
            {
                diagnostics.Add("Type '" + type.FullName + "' has an empty ButterMorph language type key.");
                continue;
            }

            discoveredTypes.Add(new ButterMorphLanguageType
            {
                Key = attribute.Key,
                DisplayName = string.IsNullOrWhiteSpace(attribute.Name) ? type.Name : attribute.Name,
                QualifiedName = type.FullName ?? type.Name,
                Version = attribute.Version ?? string.Empty,
                Description = attribute.Description ?? string.Empty,
                Source = "bundle",
                BundleKey = bundleKey
            });
        }

        if (discoveredTypes.Count == 0)
        {
            diagnostics.Add("Assembly does not contain types marked with ButterMorphLanguageTypeAttribute.");
        }

        return new ButterMorphLanguageTypeBundleDiscoveryResult
        {
            Succeeded = diagnostics.Count == 0,
            Diagnostics = diagnostics,
            Bundle = new ButterMorphLanguageTypeBundle
            {
                AssemblyId = CreateAssemblyId(assembly),
                AssemblyKey = bundleKey,
                AssemblyDescription = provider.Description ?? string.Empty,
                AssemblyVersion = provider.Version ?? string.Empty,
                LanguageKey = language,
                Types = discoveredTypes
                    .OrderBy(type => type.DisplayName, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            }
        };
    }

    private static Type[] GetLoadableTypes(Assembly assembly, List<string> diagnostics)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            diagnostics.AddRange(exception.LoaderExceptions
                .Where(item => item != null)
                .Select(item => item.Message));
            return exception.Types.Where(type => type != null).ToArray();
        }
    }

    private static string CreateAssemblyId(Assembly assembly)
    {
        Module module = assembly.ManifestModule;
        return (assembly.GetName().Name ?? "assembly") + ":" + module.ModuleVersionId.ToString("N");
    }

    private static ButterMorphLanguageTypeBundleDiscoveryResult Failed(string diagnostic)
    {
        return new ButterMorphLanguageTypeBundleDiscoveryResult
        {
            Succeeded = false,
            Diagnostics = [diagnostic]
        };
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
