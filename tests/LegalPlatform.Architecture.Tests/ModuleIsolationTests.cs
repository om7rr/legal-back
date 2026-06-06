using System.Reflection;
using LegalPlatform.Modules.Audit;
using LegalPlatform.Modules.Billing;
using LegalPlatform.Modules.Cases;
using LegalPlatform.Modules.Clients;
using LegalPlatform.Modules.CourtIntegrations;
using LegalPlatform.Modules.Documents;
using LegalPlatform.Modules.Identity;
using LegalPlatform.Modules.Notifications;
using LegalPlatform.Modules.Reporting;
using LegalPlatform.Modules.Tenancy;
using LegalPlatform.Modules.Zatca;
using NetArchTest.Rules;

namespace LegalPlatform.Architecture.Tests;

/// <summary>
/// Enforces the bounded-context boundary (ADR-0001): no module may take a compile-time dependency on
/// another module. Cross-context communication goes through integration events only.
/// </summary>
public sealed class ModuleIsolationTests
{
    private static readonly (string Name, Assembly Assembly)[] Modules =
    [
        ("Identity", typeof(IdentityModule).Assembly),
        ("Tenancy", typeof(TenancyModule).Assembly),
        ("Clients", typeof(ClientsModule).Assembly),
        ("Cases", typeof(CasesModule).Assembly),
        ("CourtIntegrations", typeof(CourtIntegrationsModule).Assembly),
        ("Documents", typeof(DocumentsModule).Assembly),
        ("Billing", typeof(BillingModule).Assembly),
        ("Zatca", typeof(ZatcaModule).Assembly),
        ("Notifications", typeof(NotificationsModule).Assembly),
        ("Reporting", typeof(ReportingModule).Assembly),
        ("Audit", typeof(AuditModule).Assembly),
    ];

    [Fact]
    public void Modules_do_not_depend_on_each_other()
    {
        var failures = new List<string>();

        foreach (var module in Modules)
        {
            var otherNamespaces = Modules
                .Where(m => m.Name != module.Name)
                .Select(m => $"LegalPlatform.Modules.{m.Name}")
                .ToArray();

            var result = Types.InAssembly(module.Assembly)
                .ShouldNot()
                .HaveDependencyOnAny(otherNamespaces)
                .GetResult();

            if (!result.IsSuccessful)
            {
                var offenders = result.FailingTypeNames ?? Enumerable.Empty<string>();
                failures.Add($"{module.Name} depends on another module via: {string.Join(", ", offenders)}");
            }
        }

        Assert.True(failures.Count == 0, string.Join(" | ", failures));
    }
}
