using Market.Domain.Entities.DiningTables;
using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.TableReservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Market.Infrastructure.Persistence.Configurations
{
    public class TableReservationConfiguration : IEntityTypeConfiguration<TableReservation>
    {
        public void Configure(EntityTypeBuilder<TableReservation> builder)
        {
            // Table name
            builder.ToTable("TableReservations");

            // Primary key
            builder.HasKey(tr => tr.Id);

            // Properties
            builder.Property(tr => tr.FirstName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(tr => tr.LastName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(tr => tr.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(tr => tr.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(tr => tr.NumberOfGuests)
                   .IsRequired();

            builder.Property(tr => tr.Notes)
                   .HasMaxLength(500)
                   .IsRequired(false);

            builder.Property(tr => tr.ReservationStart)
                   .IsRequired();

            builder.Property(tr => tr.ReservationEnd)
                   .IsRequired(false); 

            builder.Property(tr => tr.Status)
                   .IsRequired();

            builder.HasOne(tr => tr.DiningTable)
                   .WithMany(dt => dt.Reservations)
                   .HasForeignKey(tr => tr.DiningTableId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tr => tr.ApplicationUser)
                   .WithMany() 
                   .HasForeignKey(tr => tr.ApplicationUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(tr => new { tr.DiningTableId, tr.ReservationStart })
                   .IsUnique(false);

            builder.HasIndex(tr => tr.Status);
        }
    }
}
