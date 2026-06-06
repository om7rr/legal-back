namespace LegalPlatform.SharedKernel.Classification;

/// <summary>
/// Sensitivity classification for every persisted entity. Drives access control,
/// logging/redaction, retention and residency. See docs/security/data-classification.md.
/// </summary>
public enum DataClassification
{
    /// <summary>Freely shareable (e.g. public marketing content).</summary>
    Public = 0,

    /// <summary>Internal operational data, not personal or privileged.</summary>
    Internal = 1,

    /// <summary>Sensitive business data; restricted to authorised roles.</summary>
    Confidential = 2,

    /// <summary>Attorney-client privileged material. Never leaves KSA; strict access.</summary>
    LegalPrivileged = 3,

    /// <summary>Regulated personal data (PDPL). KSA-resident; subject to data-subject rights.</summary>
    RegulatedPersonalData = 4,
}
