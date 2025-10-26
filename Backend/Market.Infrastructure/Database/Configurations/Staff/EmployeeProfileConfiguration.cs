using Market.Domain.Entities.Staff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Infrastructure.Database.Configurations.Staff
{
    internal class EmployeeProfileConfiguration : IEntityTypeConfiguration<EmployeeProfile>
    {
        public void Configure(EntityTypeBuilder<EmployeeProfile> b)
        {
            b.ToTable("EmployeeProfiles");
            b.HasKey(x => x.Id);

            b.Property(x => x.FirstName).HasMaxLength(100);
            b.Property(x => x.LastName).HasMaxLength(100);
            b.Property(x => x.Position).HasMaxLength(50);
            b.Property(x => x.PhoneNumber).HasMaxLength(50);

            b.HasOne(x => x.AppUser)
                .WithOne()
                .HasForeignKey<EmployeeProfile>(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.AppUserId }).IsUnique();
        }
    }
}
