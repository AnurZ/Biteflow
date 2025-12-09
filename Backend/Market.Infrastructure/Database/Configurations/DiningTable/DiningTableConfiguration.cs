using Market.Domain.Common.Enums;
using Market.Domain.Entities.DiningTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Market.Infrastructure.Persistence.Configurations
{
    public class DiningTableConfiguration : IEntityTypeConfiguration<DiningTable>
    {
        public void Configure(EntityTypeBuilder<DiningTable> builder)
        {
            // Table name
            builder.ToTable("DiningTables");

            // Key
            builder.HasKey(t => t.Id);

            // Properties
            builder.Property(t => t.SectionName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Number)
                .IsRequired();

            builder.Property(t => t.NumberOfSeats)
                .IsRequired();

            builder.Property(t => t.IsActive)
                .HasDefaultValue(true);

            builder.Property(t => t.TableLayoutId)
                .IsRequired();

            builder.Property(t => t.X)
                .IsRequired();

            builder.Property(t => t.Y)
                .IsRequired();

            builder.Property(t => t.TableSize)
                .HasDefaultValue(100)
                .IsRequired();

            builder.Property(t => t.Shape)
                .HasMaxLength(20)
                .HasDefaultValue("rectangle")
                .IsRequired();

            builder.Property(t => t.Color)
                .HasMaxLength(20)
                .HasDefaultValue("#00ff00")
                .IsRequired();

            builder.Property(t => t.TableType)
                .IsRequired();

            builder.Property(t => t.Status)
                .HasDefaultValue(TableStatus.Free)
                .IsRequired();

            builder.Property(t => t.LastUsedAt)
                .IsRequired(false);

            // -------------------------
            // RELATIONSHIPS
            // -------------------------

            builder.HasOne(t => t.TableLayout)
                .WithMany(l => l.Tables)
                .HasForeignKey(t => t.TableLayoutId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiningTables_TableLayouts_TableLayoutId"); // explicit name


            builder.HasMany(t => t.Reservations)
                .WithOne(r => r.DiningTable)
                .HasForeignKey(r => r.DiningTableId)
                .OnDelete(DeleteBehavior.Cascade);  

            builder.HasMany(t => t.Orders)
                .WithOne(o => o.DiningTable)
                .HasForeignKey(o => o.DiningTableId)
                .OnDelete(DeleteBehavior.Cascade);   


        }
    }
}
