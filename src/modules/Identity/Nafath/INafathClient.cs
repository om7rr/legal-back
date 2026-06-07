namespace LegalPlatform.Modules.Identity.Nafath;

public enum NafathStatus
{
    Pending = 0,
    Completed = 1,
    Rejected = 2,
}

/// <summary>Result of starting a Nafath auth transaction.</summary>
public sealed record NafathInitiation(Guid TransactionId, string Number);

/// <summary>Current state of a transaction (plus the national id once known).</summary>
public sealed record NafathTransaction(Guid TransactionId, string NationalId, NafathStatus Status);

/// <summary>
/// The Nafath identity provider. The real adapter calls Nafath; <see cref="MockNafathClient"/>
/// simulates it (ADR-0006). Same interface either way.
/// </summary>
public interface INafathClient
{
    Task<NafathInitiation> InitiateAsync(string nationalId, CancellationToken cancellationToken = default);

    Task<NafathTransaction?> GetAsync(Guid transactionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Mock-only: simulates the user approving/rejecting on their Nafath app. Implemented solely by the
/// mock; the simulate-confirm endpoint that uses it is mapped only outside Production.
/// </summary>
public interface INafathSimulator
{
    Task<bool> ConfirmAsync(Guid transactionId, bool accept, CancellationToken cancellationToken = default);
}
