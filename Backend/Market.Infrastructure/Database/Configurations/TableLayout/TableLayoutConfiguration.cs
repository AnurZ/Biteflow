using Market.Domain.Entities.TableLayout;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Market.Infrastructure.Persistence.Configurations
{
    public class TableLayoutConfiguration : IEntityTypeConfiguration<TableLayout>
    {
        public void Configure(EntityTypeBuilder<TableLayout> builder)
        {
            builder.ToTable("TableLayouts");

            builder.HasKey(t => t.Id);


            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.BackgroundColor)
                .HasMaxLength(20)
                .HasDefaultValue("#ffffff");

            builder.Property(t => t.FloorImageUrl)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property(t => t.RestaurantId)
                .IsRequired(false);

            builder.HasIndex(t => t.TenantId);
            builder.HasIndex(t => t.RestaurantId);
        }
    }
}
