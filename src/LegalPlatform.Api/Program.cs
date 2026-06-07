using System.Text;
using LegalPlatform.Api;
using LegalPlatform.Api.Health;
using LegalPlatform.Api.Middleware;
using LegalPlatform.Api.Multitenancy;
using LegalPlatform.Api.RateLimiting;
using LegalPlatform.Infrastructure;
using LegalPlatform.Modules.Audit;
using LegalPlatform.Modules.Cases;
using LegalPlatform.Modules.Cases.Api;
using LegalPlatform.Modules.Identity;
using LegalPlatform.Modules.Identity.Api;
using LegalPlatform.Modules.Identity.Application;
using LegalPlatform.Modules.Identity.Auth;
using LegalPlatform.SharedKernel.Multitenancy;
using LegalPlatform.SharedKernel.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, configuration) => configuration
        .Enrich.FromLogContext()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .WriteTo.Console(new CompactJsonFormatter()));

    // Bounded-context assemblies that own EF configurations (see ModuleCatalog — shared with the
    // design-time migrations factory so they never drift).
    builder.Services.AddInfrastructure(builder.Configuration, ModuleCatalog.Assemblies);

    builder.Services.AddScoped<TenantContext>();
    builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

    builder.Services.AddCasesModule();
    builder.Services.AddAuditModule();
    builder.Services.AddIdentityModule(builder.Configuration, builder.Environment);

    // JWT bearer auth — keys/issuer/audience resolved once (ADR-0006).
    var jwt = JwtSetup.Resolve(builder.Configuration, builder.Environment);
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                ValidateLifetime = true,
                RoleClaimType = "role",
                NameClaimType = "sub",
            };
        });
    builder.Services.AddAuthorization();

    builder.Services.AddScoped<PostgresHealthCheck>();
    builder.Services.AddScoped<RedisHealthCheck>();
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("API is running."), tags: new[] { "live" })
        .AddCheck<PostgresHealthCheck>("postgres", tags: new[] { "ready" })
        .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready" });

    builder.Services.AddPlatformRateLimiter();

    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod()));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<TenantResolverMiddleware>();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live"),
        ResponseWriter = HealthResponseWriter.WriteJson,
    }).DisableRateLimiting();

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = HealthResponseWriter.WriteJson,
    }).DisableRateLimiting();

    app.MapGet("/", () => Results.Ok(new { name = "Legal Platform API", status = "ok" }));

    // Auth flow is anonymous; the simulate-confirm endpoint is exposed only outside Production.
    app.MapAuthEndpoints(enableSimulator: !app.Environment.IsProduction());
    app.MapCasesEndpoints();

    // Seed the test environment's firms/users (idempotent). Best-effort: a missing DB must not crash startup.
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
            await IdentitySeeder.SeedAsync(db);
        }
        catch (Exception seedEx)
        {
            Log.Warning(seedEx, "Identity seed skipped (database not ready).");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Exposed so integration tests can host the API via WebApplicationFactory.</summary>
public partial class Program;
