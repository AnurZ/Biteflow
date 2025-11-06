namespace Market.Application.Modules.Auth.Commands.Refresh;

using Market.Domain.Entities.IdentityV2;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenCommandDto>
{
    private readonly IAppDbContext _ctx;
    private readonly IJwtTokenService _jwt;
    private readonly TimeProvider _timeProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public RefreshTokenCommandHandler(
        IAppDbContext ctx,
        IJwtTokenService jwt,
        TimeProvider timeProvider,
        UserManager<ApplicationUser> userManager)
    {
        _ctx = ctx;
        _jwt = jwt;
        _timeProvider = timeProvider;
        _userManager = userManager;
    }

    public async Task<RefreshTokenCommandDto> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var incomingHash = _jwt.HashRefreshToken(request.RefreshToken);

        var rt = await _ctx.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.TokenHash == incomingHash &&
                !x.IsRevoked &&
                !x.IsDeleted, ct);

        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

        if (rt is null || rt.ExpiresAtUtc <= nowUtc)
            throw new MarketConflictException("Refresh token je nevazeci ili je istekao.");

        if (rt.Fingerprint is not null &&
            request.Fingerprint is not null &&
            rt.Fingerprint != request.Fingerprint)
        {
            throw new MarketConflictException("Neispravan klijentski otisak.");
        }

        var user = rt.User;
        if (user is null || !user.IsEnabled || user.IsDeleted)
            throw new MarketConflictException("Korisnicki nalog je nevazeci.");

        rt.IsRevoked = true;
        rt.RevokedAtUtc = nowUtc;

        var identityUser = await _userManager.FindByNameAsync(user.Email)
            ?? await _userManager.FindByEmailAsync(user.Email)
            ?? await _userManager.FindByEmailAsync($"{user.Email}@legacy.local");

        var roles = identityUser is null
            ? Array.Empty<string>()
            : await _userManager.GetRolesAsync(identityUser);

        var pair = _jwt.IssueTokens(user, roles);

        var newRt = new RefreshTokenEntity
        {
            TokenHash = pair.RefreshTokenHash,
            ExpiresAtUtc = pair.RefreshTokenExpiresAtUtc,
            UserId = user.Id,
            Fingerprint = request.Fingerprint,
        };

        _ctx.RefreshTokens.Add(newRt);
        await _ctx.SaveChangesAsync(ct);

        return new RefreshTokenCommandDto
        {
            AccessToken = pair.AccessToken,
            RefreshToken = pair.RefreshTokenRaw,
            AccessTokenExpiresAtUtc = pair.AccessTokenExpiresAtUtc,
            RefreshTokenExpiresAtUtc = pair.RefreshTokenExpiresAtUtc
        };
    }
}
