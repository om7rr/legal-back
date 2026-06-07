using LegalPlatform.Modules.Identity.Auth;
using LegalPlatform.Modules.Identity.Contracts;
using LegalPlatform.Modules.Identity.Domain;
using LegalPlatform.Modules.Identity.Nafath;
using LegalPlatform.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LegalPlatform.Modules.Identity.Application;

/// <summary>Orchestrates the Nafath flow and issues JWTs for verified, provisioned users (ADR-0006).</summary>
public sealed class NafathAuthService
{
    private readonly INafathClient _nafath;
    private readonly INafathSimulator _simulator;
    private readonly IAppDbContext _db;
    private readonly IJwtTokenIssuer _jwt;
    private readonly IRefreshTokenStore _refreshTokens;

    public NafathAuthService(
        INafathClient nafath,
        INafathSimulator simulator,
        IAppDbContext db,
        IJwtTokenIssuer jwt,
        IRefreshTokenStore refreshTokens)
    {
        _nafath = nafath;
        _simulator = simulator;
        _db = db;
        _jwt = jwt;
        _refreshTokens = refreshTokens;
    }

    public async Task<InitiateResponse> InitiateAsync(string nationalId, CancellationToken cancellationToken = default)
    {
        var initiation = await _nafath.InitiateAsync(nationalId, cancellationToken);
        return new InitiateResponse(
            initiation.TransactionId,
            initiation.Number,
            $"Open your Nafath app and select the number {initiation.Number}.");
    }

    public async Task<AuthStatusResponse> GetStatusAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _nafath.GetAsync(transactionId, cancellationToken);
        if (transaction is null)
        {
            return new AuthStatusResponse("NotFound", Message: "Unknown transaction.");
        }

        if (transaction.Status != NafathStatus.Completed)
        {
            return new AuthStatusResponse(transaction.Status.ToString());
        }

        var user = await _db.Set<AppUser>()
            .FirstOrDefaultAsync(u => u.NationalId == transaction.NationalId, cancellationToken);

        if (user is null)
        {
            return new AuthStatusResponse(
                "Completed",
                Message: "Identity verified, but no user is provisioned for this national id.");
        }

        return IssueTokens(user);
    }

    public async Task<TokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (!_refreshTokens.TryConsume(refreshToken, out var userId))
        {
            return null;
        }

        var user = await _db.Set<AppUser>().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var access = _jwt.Create(user);
        return new TokenResponse(access.Token, _refreshTokens.Issue(user.Id), access.ExpiresInSeconds);
    }

    public Task<bool> SimulateConfirmAsync(Guid transactionId, bool accept, CancellationToken cancellationToken = default)
        => _simulator.ConfirmAsync(transactionId, accept, cancellationToken);

    private AuthStatusResponse IssueTokens(AppUser user)
    {
        var access = _jwt.Create(user);
        var refresh = _refreshTokens.Issue(user.Id);
        return new AuthStatusResponse("Completed", access.Token, refresh, access.ExpiresInSeconds);
    }
}
