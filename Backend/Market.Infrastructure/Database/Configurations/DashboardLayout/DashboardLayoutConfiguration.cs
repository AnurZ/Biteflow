namespace Market.Infrastructure.Database.Configurations.DashboardLayout
{
    public class DashboardLayoutConfiguration
        : IEntityTypeConfiguration<Market.Domain.Entities.DashboardLayout.DashboardLayout>
    {
        public void Configure(
            EntityTypeBuilder<Market.Domain.Entities.DashboardLayout.DashboardLayout> builder)
        {
            builder.ToTable("DashboardLayouts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.LayoutJson)
                .IsRequired();

            builder.HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ApplicationUserId)
                .IsUnique();
        }
    }
}