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
                BuildProfileResource(),
                new IdentityResources.Email(),
                new IdentityResource("roles", new[] { JwtClaimTypes.Role })
            };

        private static IdentityResource BuildProfileResource()
        {
            var profile = new IdentityResources.Profile();
            profile.UserClaims.Add("display_name");
            profile.UserClaims.Add("restaurant_id");
            profile.UserClaims.Add("tenant_id");
            return profile;
        }

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
            new ApiScope("biteflow.api", "Biteflow API", new[]
            {
                JwtClaimTypes.Role, JwtClaimTypes.Email, "restaurant_id", "tenant_id", "display_name"
            })
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new[]
            {
            new ApiResource("biteflow.api", "Biteflow API")
            {
                Scopes = { "biteflow.api" },
                UserClaims = { JwtClaimTypes.Role, JwtClaimTypes.Email, "restaurant_id", "tenant_id", "display_name" }
            }
            };

        public static IEnumerable<Client> Clients =>
            new[]
            {
            new Client
            {
                ClientId = "biteflow-angular",
                ClientName = "Biteflow Angular SPA",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                RequireClientSecret = false,
                AllowOfflineAccess = true,
                AllowedCorsOrigins =
                {
                    "http://localhost:4200",
                    "https://localhost:4200"
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "roles",
                    "biteflow.api",
                    IdentityServerConstants.StandardScopes.OfflineAccess
                },
                AccessTokenLifetime = 1200,
                AlwaysIncludeUserClaimsInIdToken = false
            }
            };
    }
}
