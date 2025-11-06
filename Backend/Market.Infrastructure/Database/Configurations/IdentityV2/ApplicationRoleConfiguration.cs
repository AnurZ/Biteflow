using Market.Domain.Entities.IdentityV2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Infrastructure.Database.Configurations.IdentityV2
{
    public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.ToTable("AspNetRoles");
            builder.Property(x => x.Name).HasMaxLength(256);
            builder.Property(x => x.NormalizedName).HasMaxLength(256);
        }
    }
}
