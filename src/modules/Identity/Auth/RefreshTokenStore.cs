using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace LegalPlatform.Modules.Identity.Auth;

public interface IRefreshTokenStore
{
    string Issue(Guid userId);

    bool TryConsume(string refreshToken, out Guid userId);
}

/// <summary>
/// In-memory refresh-token store for the test environment (ADR-0006). Single-use (rotated on consume).
/// Production persists tokens with expiry, rotation, and revocation.
/// </summary>
public sealed class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    private readonly ConcurrentDictionary<string, Guid> _tokens = new(StringComparer.Ordinal);

    public string Issue(Guid userId)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _tokens[token] = userId;
        return token;
    }

    public bool TryConsume(string refreshToken, out Guid userId)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            userId = Guid.Empty;
            return false;
        }

        return _tokens.TryRemove(refreshToken, out userId);
    }
}
