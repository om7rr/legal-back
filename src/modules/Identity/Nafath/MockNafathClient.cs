using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace LegalPlatform.Modules.Identity.Nafath;

/// <summary>
/// In-memory simulation of Nafath for the test environment (ADR-0006). Holds transactions in memory
/// (singleton); a transaction starts Pending and is moved to Completed/Rejected by the simulator.
/// No external calls. Not for production.
/// </summary>
public sealed class MockNafathClient : INafathClient, INafathSimulator
{
    private sealed class Entry
    {
        public required string NationalId { get; init; }
        public required string Number { get; init; }
        public NafathStatus Status { get; set; } = NafathStatus.Pending;
    }

    private readonly ConcurrentDictionary<Guid, Entry> _transactions = new();

    public Task<NafathInitiation> InitiateAsync(string nationalId, CancellationToken cancellationToken = default)
    {
        var transactionId = Guid.NewGuid();
        // Two-digit number the user would match in their Nafath app.
        var number = RandomNumberGenerator.GetInt32(10, 100).ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
        _transactions[transactionId] = new Entry { NationalId = nationalId.Trim(), Number = number };
        return Task.FromResult(new NafathInitiation(transactionId, number));
    }

    public Task<NafathTransaction?> GetAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        if (_transactions.TryGetValue(transactionId, out var entry))
        {
            return Task.FromResult<NafathTransaction?>(
                new NafathTransaction(transactionId, entry.NationalId, entry.Status));
        }

        return Task.FromResult<NafathTransaction?>(null);
    }

    public Task<bool> ConfirmAsync(Guid transactionId, bool accept, CancellationToken cancellationToken = default)
    {
        if (!_transactions.TryGetValue(transactionId, out var entry))
        {
            return Task.FromResult(false);
        }

        entry.Status = accept ? NafathStatus.Completed : NafathStatus.Rejected;
        return Task.FromResult(true);
    }
}
