using Microsoft.AspNetCore.Identity;

namespace Market.Domain.Entities.IdentityV2;

public class ApplicationRole : IdentityRole<Guid>
{
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
}
