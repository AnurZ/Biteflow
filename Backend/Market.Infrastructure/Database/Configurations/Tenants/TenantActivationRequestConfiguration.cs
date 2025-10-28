using Market.Domain.Entities.Tenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Infrastructure.Database.Configurations.Tenants
{
    public sealed class TenantActivationRequestConfiguration : IEntityTypeConfiguration<TenantActivationRequest>
    {
        public void Configure(EntityTypeBuilder<TenantActivationRequest> b)
        {
            b.ToTable("TenantActivationRequest");
            b.HasKey(x => x.Id);

            b.Property(x => x.RestaurantName).IsRequired().HasMaxLength(200);
            b.Property(x => x.Domain).IsRequired().HasMaxLength(120);
            b.Property(x => x.OwnerFullName).IsRequired().HasMaxLength(120);
            b.Property(x => x.OwnerEmail).IsRequired().HasMaxLength(256);
            b.Property(x => x.OwnerPhone).IsRequired().HasMaxLength(40);
            b.Property(x => x.Address).IsRequired().HasMaxLength(200);
            b.Property(x => x.City).IsRequired().HasMaxLength(80);
            b.Property(x => x.State).IsRequired().HasMaxLength(80);

            b.Property(x => x.Status).HasConversion<int>();
            b.HasIndex(x => x.Domain).IsUnique();
        }
    }
}
