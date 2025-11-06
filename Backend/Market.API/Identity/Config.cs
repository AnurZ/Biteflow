using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Market.API.Identity
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", new[] { JwtClaimTypes.Role })
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
            new ApiScope("biteflow.api", "Biteflow API", new[]
            {
                JwtClaimTypes.Role, "restaurant_id", "tenant_id", "display_name"
            })
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new[]
            {
            new ApiResource("biteflow.api", "Biteflow API")
            {
                Scopes = { "biteflow.api" },
                UserClaims = { JwtClaimTypes.Role, "restaurant_id", "tenant_id", "display_name" }
            }
            };

        public static IEnumerable<Client> Clients =>
            new[]
            {
            new Client
            {
                ClientId = "biteflow-angular",
                ClientName = "Biteflow Angular SPA",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = { "https://localhost:4200/auth/callback" },
                PostLogoutRedirectUris = { "https://localhost:4200/" },
                AllowedCorsOrigins = { "https://localhost:4200" },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "roles",
                    "biteflow.api"
                },
                AccessTokenLifetime = 1200, 
                AlwaysIncludeUserClaimsInIdToken = false
            }
            };
    }
}
