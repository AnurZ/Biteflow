using Market.Domain.Entities.Meal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Market.Infrastructure.Database.Configurations.Meals
{
    internal class MealConfiguration : IEntityTypeConfiguration<Meal>
    {
        public void Configure(EntityTypeBuilder<Meal> builder)
        {
            builder.ToTable("Meals");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Description)
                .HasMaxLength(500);

            builder.Property(m => m.BasePrice)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(m => m.IsAvailable)
                .HasDefaultValue(true);

            builder.Property(m => m.IsFeatured)
                .HasDefaultValue(false);

            builder.Property(m => m.ImageField)
                .HasMaxLength(250);

            builder.Property(m => m.StockManaged)
                .HasDefaultValue(true);

        }
    }
}
