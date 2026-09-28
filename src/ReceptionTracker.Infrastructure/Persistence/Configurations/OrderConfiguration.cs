using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Reference).HasMaxLength(50).IsRequired();
        builder.HasIndex(o => o.Reference).IsUnique();

        builder.HasMany(o => o.Pallets)
            .WithOne()
            .HasForeignKey("OrderId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Computed from the product lines, never stored.
        builder.Ignore(o => o.Status);
        builder.Ignore(o => o.Products);
    }
}
