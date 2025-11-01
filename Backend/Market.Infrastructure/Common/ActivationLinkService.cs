using Market.Application.Abstractions;
using Market.Application.Common.Exceptions;
using Market.Domain.Entities.ActivationLinkEntity;
using Market.Shared.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Market.Infrastructure.Common
{
    public sealed class ActivationLinkService : IActivationLinkService
    {
        private readonly IAppDbContext _db;
        private readonly ActivationLinkOptions _opts;

        public ActivationLinkService(IAppDbContext db, IOptions<ActivationLinkOptions> opts)
        {
            _db = db;
            _opts = opts.Value;
            if (string.IsNullOrWhiteSpace(_opts.TokenSecret))
                throw new InvalidOperationException("ActivationLinkOptions.TokenSecret is not configured.");
        }

        public async Task<string> IssueLinkAsync(int requestId, CancellationToken ct)
        {
            var req = await _db.TenantActivationRequests
                .FindAsync(new object[] { requestId }, ct)
                ?? throw new MarketNotFoundException("Request not found");

            var rawToken = GenerateUrlSafeToken(32);
            var tokenHash = ComputeHmacSha256Hex(rawToken, _opts.TokenSecret);

            var now = DateTimeOffset.UtcNow;
            var existing = await _db.ActivationLinks
                .Where(x => x.RequestId == requestId && x.ConsumedAtUtc == null && x.ExpiresAtUtc > now)
                .ToListAsync(ct);

            foreach (var x in existing) x.ExpiresAtUtc = now;

            var link = new ActivationLinkEntity
            {
                RequestId = requestId,
                TokenHash = tokenHash,
                ExpiresAtUtc = now.Add(_opts.Lifetime),
                IssuedBy = "system"
            };

            _db.ActivationLinks.Add(link);
            await _db.SaveChangesAsync(ct);

            return ComposeUrlWithToken(rawToken);
        }


        public async Task<int> ValidateAndConsumeAsync(string token, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Invalid token.");

            var tokenHash = ComputeHmacSha256Hex(token, _opts.TokenSecret);

            // Check if token is expired
            var now = DateTimeOffset.UtcNow;
            var link = await _db.ActivationLinks
                .Where(x => x.TokenHash == tokenHash)
                .SingleOrDefaultAsync(ct);

            if (link is null || link.ConsumedAtUtc != null || link.ExpiresAtUtc <= now)
                throw new UnauthorizedAccessException("Activation link is invalid or expired.");

            link.ConsumedAtUtc = now;

            _db.ActivationLinks.Update(link);
            await _db.SaveChangesAsync(ct);

            return link.RequestId;
        }

        private string ComposeUrlWithToken(string token)
        {
            var baseUrl = _opts.BaseUrl.TrimEnd('/');
            var route = _opts.Route.StartsWith('/') ? _opts.Route : "/" + _opts.Route;
            return $"{baseUrl}{route}?token={WebUtility.UrlEncode(token)}";
        }

        private static string GenerateUrlSafeToken(int bytes)
        {
            Span<byte> buffer = stackalloc byte[bytes];
            RandomNumberGenerator.Fill(buffer);
            // base64url
            var b64 = Convert.ToBase64String(buffer).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return b64;
        }

        private static string ComputeHmacSha256Hex(string input, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
