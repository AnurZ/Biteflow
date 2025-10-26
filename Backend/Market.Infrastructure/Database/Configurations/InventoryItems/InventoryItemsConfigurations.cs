using Market.Domain.Entities.InventoryItem;
using Market.Domain.Entities.Staff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Infrastructure.Database.Configurations.InventoryItems
{
    internal class InventoryItemsConfigurations : IEntityTypeConfiguration<InventoryItem>
    {
        public void Configure(EntityTypeBuilder<InventoryItem> builder)
        {
            builder.ToTable("InventoryItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RestaurantId)
                .IsRequired();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Sku)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.UnitType)
                .IsRequired()
                .HasConversion<string>(); 

            builder.Property(x => x.ReorderQty)
                .IsRequired();

            builder.Property(x => x.ReorderFrequency)
                .IsRequired();

            builder.Property(x => x.CurrentQty)
                .IsRequired();

            builder.HasIndex(x => x.Sku).IsUnique();
            builder.HasIndex(x => x.RestaurantId);
        }
    }
}
