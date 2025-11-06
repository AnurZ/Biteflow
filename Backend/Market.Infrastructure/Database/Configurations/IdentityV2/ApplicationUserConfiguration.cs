using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.Staff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Infrastructure.Database.Configurations.IdentityV2
{
    public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("AspNetUsers");
            builder.Property(x => x.DisplayName)
                   .HasMaxLength(256);

            builder.HasOne(x => x.EmployeeProfile)
                   .WithOne(x => x.ApplicationUser)
                   .HasForeignKey<EmployeeProfile>(x => x.ApplicationUserId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
