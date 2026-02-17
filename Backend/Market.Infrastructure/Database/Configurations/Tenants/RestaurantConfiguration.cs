using Market.Domain.Entities.Tenants;

namespace Market.Infrastructure.Database.Configurations.Tenants;

public sealed class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("Restaurants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Domain)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.State)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.Domain)
            .IsUnique();

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.Restaurants)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
