using Market.Domain.Common.Enums;
using Market.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Market.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Status)
                .HasDefaultValue(OrderStatus.New)
                .IsRequired();

            builder.Property(o => o.TableNumber)
                .IsRequired(false);

            builder.Property(o => o.Notes)
                .HasMaxLength(1024)
                .IsRequired(false);

            builder.HasOne(o => o.DiningTable)
                .WithMany(t => t.Orders)
                .HasForeignKey(o => o.DiningTableId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
