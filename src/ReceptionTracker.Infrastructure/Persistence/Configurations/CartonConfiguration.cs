using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Infrastructure.Persistence.Configurations;

internal sealed class CartonConfiguration : IEntityTypeConfiguration<Carton>
{
    public void Configure(EntityTypeBuilder<Carton> builder)
    {
        builder.ToTable("Cartons");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex("PalletId", nameof(Carton.Code)).IsUnique();

        builder.HasMany(c => c.Products)
            .WithOne()
            .HasForeignKey("CartonId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(c => c.Status);
    }
}
