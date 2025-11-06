
using Market.Domain.Entities.IdentityV2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Infrastructure.Database.Configurations.IdentityV2
{
    public sealed class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
        {
            builder.ToTable("AspNetUserRoles");

            builder.HasKey(x => new { x.UserId, x.RoleId });

            builder.HasOne(x => x.User)
                   .WithMany(x => x.UserRoles)
                   .HasForeignKey(x => x.UserId);

            builder.HasOne(x => x.Role)
                   .WithMany(x => x.UserRoles)
                   .HasForeignKey(x => x.RoleId);
        }
    }
}
