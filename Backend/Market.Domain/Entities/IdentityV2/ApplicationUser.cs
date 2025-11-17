using Market.Domain.Entities.Staff;
using Microsoft.AspNetCore.Identity;

namespace Market.Domain.Entities.IdentityV2;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }
    public Guid? RestaurantId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int TokenVersion { get; set; }

    public EmployeeProfile? EmployeeProfile { get; set; }
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
}
