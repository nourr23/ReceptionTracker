using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Infrastructure.Persistence.Configurations;

internal sealed class ProductLineConfiguration : IEntityTypeConfiguration<ProductLine>
{
    public void Configure(EntityTypeBuilder<ProductLine> builder)
    {
        builder.ToTable("ProductLines", t =>
            t.HasCheckConstraint("CK_ProductLines_ExpectedQuantity_Positive", "\"ExpectedQuantity\" > 0"));
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Reference).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Color).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Size).HasMaxLength(20).IsRequired();
        builder.Property(p => p.ExpectedQuantity).IsRequired();
        builder.Property(p => p.IsReceived).IsRequired();
    }
}
