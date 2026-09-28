using Microsoft.EntityFrameworkCore;
using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Infrastructure.Persistence.Seed;

/// <summary>
/// Fake pending orders, inserted when the database is migrated and still empty.
/// Wired through EF Core's UseSeeding / UseAsyncSeeding (both are required by EF).
/// </summary>
internal static class OrderSeeder
{
    public static void Seed(DbContext context)
    {
        if (context.Set<Order>().Any())
        {
            return;
        }

        context.Set<Order>().AddRange(CreateOrders());
        context.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        if (await context.Set<Order>().AnyAsync(cancellationToken))
        {
            return;
        }

        context.Set<Order>().AddRange(CreateOrders());
        await context.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<Order> CreateOrders()
    {
        var cmd2026 = new Order("CMD-2026");

        var pal01 = cmd2026.AddPallet("PAL-01");
        var cart01A = pal01.AddCarton("CART-01-A");
        cart01A.AddProduct("TSH-RED-M", "T-Shirt Sport", "Rouge", "M", 50);
        cart01A.AddProduct("SHO-BLK-42", "Baskets Running", "Noir", "42", 10);
        var cart01B = pal01.AddCarton("CART-01-B");
        cart01B.AddProduct("TSH-RED-L", "T-Shirt Sport", "Rouge", "L", 40);
        cart01B.AddProduct("TSH-BLU-M", "T-Shirt Sport", "Bleu", "M", 30);

        var pal02 = cmd2026.AddPallet("PAL-02");
        var cart02A = pal02.AddCarton("CART-02-A");
        cart02A.AddProduct("SHO-BLK-43", "Baskets Running", "Noir", "43", 12);
        cart02A.AddProduct("SHO-WHT-41", "Baskets Running", "Blanc", "41", 8);
        var cart02B = pal02.AddCarton("CART-02-B");
        cart02B.AddProduct("BAL-FOOT-5", "Ballon de Football", "Blanc", "5", 20);
        cart02B.AddProduct("SRT-BLK-M", "Short de Sport", "Noir", "M", 25);
        var cart02C = pal02.AddCarton("CART-02-C");
        cart02C.AddProduct("GRD-BLK-TU", "Gourde Isotherme", "Noir", "TU", 60);

        var cmd2027 = new Order("CMD-2027");

        var pal03 = cmd2027.AddPallet("PAL-01");
        var cart03A = pal03.AddCarton("CART-01-A");
        cart03A.AddProduct("RAQ-TEN-L2", "Raquette de Tennis", "Jaune", "L2", 6);
        cart03A.AddProduct("BAL-TEN-X4", "Balles de Tennis (x4)", "Jaune", "TU", 48);
        var cart03B = pal03.AddCarton("CART-01-B");
        cart03B.AddProduct("LEG-BLK-S", "Legging Running", "Noir", "S", 15);
        cart03B.AddProduct("LEG-BLK-M", "Legging Running", "Noir", "M", 20);

        return [cmd2026, cmd2027];
    }
}
