using System.Reflection;

namespace LegalPlatform.Infrastructure.Persistence;

/// <summary>
/// The set of bounded-context assemblies whose EF configurations the shared <see cref="AppDbContext"/>
/// discovers at model-build time. Supplied by the composition root (the API) so Infrastructure stays
/// decoupled from individual modules — it never references a specific context.
/// </summary>
public sealed class ModuleRegistry
{
    public ModuleRegistry(IEnumerable<Assembly> moduleAssemblies)
    {
        ArgumentNullException.ThrowIfNull(moduleAssemblies);
        ModuleAssemblies = moduleAssemblies.Distinct().ToArray();
    }

    public IReadOnlyList<Assembly> ModuleAssemblies { get; }
}
