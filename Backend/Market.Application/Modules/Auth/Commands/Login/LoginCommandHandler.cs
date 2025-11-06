using Market.Application.Modules.Auth.Commands.Login;
using Market.Domain.Entities.IdentityV2;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandDto>
{
    private readonly IAppDbContext _ctx;
    private readonly IJwtTokenService _jwt;
    private readonly IPasswordHasher<AppUser> _hasher;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginCommandHandler(
        IAppDbContext ctx,
        IJwtTokenService jwt,
        IPasswordHasher<AppUser> hasher,
        UserManager<ApplicationUser> userManager)
    {
        _ctx = ctx;
        _jwt = jwt;
        _hasher = hasher;
        _userManager = userManager;
    }

    public async Task<LoginCommandDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _ctx.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email && x.IsEnabled && !x.IsDeleted, ct)
            ?? throw new MarketNotFoundException("Korisnik nije pronaden ili je onemogucen.");

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
            throw new MarketConflictException("Pogresni kredencijali.");

        var identityUser = await _userManager.FindByEmailAsync(user.Email);
        var roles = identityUser is null
            ? Array.Empty<string>()
            : await _userManager.GetRolesAsync(identityUser);

        var tokens = _jwt.IssueTokens(user, roles);

        _ctx.RefreshTokens.Add(new RefreshTokenEntity
        {
            TokenHash = tokens.RefreshTokenHash,
            ExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc,
            UserId = user.Id,
            Fingerprint = request.Fingerprint
        });

        await _ctx.SaveChangesAsync(ct);

        return new LoginCommandDto
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshTokenRaw,
            ExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc
        };
    }
}
