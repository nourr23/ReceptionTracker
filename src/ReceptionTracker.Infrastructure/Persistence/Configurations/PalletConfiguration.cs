using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Infrastructure.Persistence.Configurations;

internal sealed class PalletConfiguration : IEntityTypeConfiguration<Pallet>
{
    public void Configure(EntityTypeBuilder<Pallet> builder)
    {
        builder.ToTable("Pallets");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex("OrderId", nameof(Pallet.Code)).IsUnique();

        builder.HasMany(p => p.Cartons)
            .WithOne()
            .HasForeignKey("PalletId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(p => p.Status);
        builder.Ignore(p => p.Products);
    }
}
