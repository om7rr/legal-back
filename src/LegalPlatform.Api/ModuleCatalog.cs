using System.Reflection;
using LegalPlatform.Modules.Audit;
using LegalPlatform.Modules.Cases;
using LegalPlatform.Modules.Identity;
using LegalPlatform.Modules.Tenancy;

namespace LegalPlatform.Api;

/// <summary>
/// Single source of truth for the bounded-context assemblies that own EF configurations. Used by
/// both the runtime composition root (Program) and the design-time factory, so migrations and the
/// running app never drift. Add a module's marker here when it gains persisted entities.
/// </summary>
public static class ModuleCatalog
{
    public static Assembly[] Assemblies =>
    [
        typeof(TenancyModule).Assembly,
        typeof(AuditModule).Assembly,
        typeof(CasesModule).Assembly,
        typeof(IdentityModule).Assembly,
    ];
}
