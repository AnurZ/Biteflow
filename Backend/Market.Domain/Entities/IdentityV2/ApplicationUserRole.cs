using Microsoft.AspNetCore.Identity;

namespace Market.Domain.Entities.IdentityV2;

public class ApplicationUserRole : IdentityUserRole<Guid>
{
    public ApplicationUser User { get; set; } = default!;
    public ApplicationRole Role { get; set; } = default!;
}
