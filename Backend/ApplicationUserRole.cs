using Microsoft.AspNetCore.Identity;
using System;

namespace Market.Domain.Entities.IdentityV2
{
    public class ApplicationUserRole : IdentityUserRole<Guid>
    {
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }
}