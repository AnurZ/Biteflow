using Market.Domain.Entities.MealCategory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Market.Infrastructure.Database.Configurations.MealCategories
{
    internal class MealCategoryConfiguration : IEntityTypeConfiguration<MealCategory>
    {
        public void Configure(EntityTypeBuilder<MealCategory> builder)
        {
            builder.ToTable("MealCategories");

            builder.HasKey(mc => mc.Id);

            builder.Property(mc => mc.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(mc => mc.Description)
                .HasMaxLength(100);

            
        }
    }
}
