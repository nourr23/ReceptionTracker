using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Application.Orders;

/// <summary>Domain → DTO mapping. Children are sorted so the API output is stable.</summary>
internal static class OrderMappings
{
    public static OrderSummaryDto ToSummaryDto(this Order order) =>
        new(order.Reference, order.Status, order.GetProgress().ToDto());

    public static OrderDto ToDto(this Order order) =>
        new(
            order.Reference,
            order.Status,
            order.GetProgress().ToDto(),
            [.. order.Pallets.OrderBy(p => p.Code).Select(ToDto)]);

    private static PalletDto ToDto(this Pallet pallet) =>
        new(
            pallet.Code,
            pallet.Status,
            pallet.GetProgress().ToDto(),
            [.. pallet.Cartons.OrderBy(c => c.Code).Select(ToDto)]);

    private static CartonDto ToDto(this Carton carton) =>
        new(
            carton.Code,
            carton.Status,
            carton.GetProgress().ToDto(),
            [.. carton.Products.OrderBy(p => p.Id).Select(ToDto)]);

    private static ProductLineDto ToDto(this ProductLine product) =>
        new(
            product.Id,
            product.Reference,
            product.Name,
            product.Color,
            product.Size,
            product.ExpectedQuantity,
            product.IsReceived);

    private static ProgressDto ToDto(this ReceptionProgress progress) =>
        new(progress.ReceivedLines, progress.TotalLines, progress.ReceivedUnits, progress.TotalUnits);
}
