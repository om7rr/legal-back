namespace LegalPlatform.Modules.Identity.Contracts;

public sealed record InitiateRequest(string NationalId);

public sealed record InitiateResponse(Guid TransactionId, string Number, string Message);

public sealed record SimulateConfirmRequest(Guid TransactionId, bool Accept);

public sealed record RefreshRequest(string RefreshToken);

/// <summary>Status of a Nafath transaction; tokens are present only once Completed for a known user.</summary>
public sealed record AuthStatusResponse(
    string Status,
    string? AccessToken = null,
    string? RefreshToken = null,
    int? ExpiresInSeconds = null,
    string? Message = null);

public sealed record TokenResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds);
