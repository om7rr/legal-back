namespace LegalPlatform.Modules.Identity.Domain;

/// <summary>RBAC roles (ADR-0006). Authorize by role + tenant, default-deny.</summary>
public enum UserRole
{
    FirmAdmin = 0,
    Lawyer = 1,
    Paralegal = 2,
    Client = 3,
}
