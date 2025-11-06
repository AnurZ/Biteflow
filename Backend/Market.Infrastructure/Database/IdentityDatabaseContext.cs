using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.Staff;
using Market.Infrastructure.Database.Configurations.IdentityV2;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Market.Infrastructure.Database
{
    public sealed class IdentityDatabaseContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>,
                        ApplicationUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>,
                        IdentityUserToken<Guid>>
    {
        public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();

        public IdentityDatabaseContext(
            DbContextOptions<IdentityDatabaseContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new ApplicationUserConfiguration());
            builder.ApplyConfiguration(new ApplicationRoleConfiguration());
            builder.ApplyConfiguration(new ApplicationUserRoleConfiguration());

            builder.Entity<EmployeeProfile>(entity =>
            {
                entity.ToTable("EmployeeProfiles", t => t.ExcludeFromMigrations());
                entity.Property(x => x.Position).HasMaxLength(128);
                entity.Property(x => x.ApplicationUserId).IsRequired(false);

                entity.HasOne(x => x.ApplicationUser)
                      .WithOne(x => x.EmployeeProfile)
                      .HasForeignKey<EmployeeProfile>(x => x.ApplicationUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
