using Market.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Market.Infrastructure.Persistence.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(oi => oi.Id);

            builder.Property(oi => oi.Name)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(oi => oi.Quantity)
                .IsRequired();

            builder.Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.HasOne(oi => oi.Meal)
                .WithMany()
                .HasForeignKey(oi => oi.MealId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
