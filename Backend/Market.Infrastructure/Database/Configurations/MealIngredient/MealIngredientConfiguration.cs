using Market.Domain.Entities.MealIngredient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Market.Infrastructure.Database.Configurations.MealIngredients
{
    internal class MealIngredientConfiguration : IEntityTypeConfiguration<MealIngredient>
    {
        public void Configure(EntityTypeBuilder<MealIngredient> builder)
        {
            builder.ToTable("MealIngredients");

            builder.HasKey(mi => mi.Id); // or composite key: { mi.MealId, mi.InventoryItemId }

            builder.Property(mi => mi.Quantity)
                .IsRequired();

            builder.Property(mi => mi.UnitTypes)
                .IsRequired()
                .HasConversion<string>();

            // Relationships
            builder.HasOne(mi => mi.Meal)
                   .WithMany(m => m.Ingredients)
                   .HasForeignKey(mi => mi.MealId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(mi => mi.InventoryItem)
                   .WithMany()
                   .HasForeignKey(mi => mi.InventoryItemId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
