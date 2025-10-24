using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Infrastructure.Database.Configurations.Identity
{
    public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> b)
        {
            b.ToTable("AppUsers");

            b.HasKey(x => x.Id);

            // Tenant and restaurant scoping
            b.HasIndex(x => new { x.TenantId, x.RestaurantId });

            // Email constraints
            b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            // Display name and security
            b.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(x => x.PasswordHash)
                .IsRequired();

            // Flags
            b.Property(x => x.IsEmailConfirmed)
                .HasDefaultValue(false);

            b.Property(x => x.IsLocked)
                .HasDefaultValue(false);

            b.Property(x => x.EncryptedSensitiveData)
                .HasColumnType("nvarchar(max)");

            // Navigation
            b.HasMany(x => x.RefreshTokens)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
