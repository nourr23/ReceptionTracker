using ReceptionTracker.Application.Common;

namespace ReceptionTracker.Application.Orders;

public static class OrderErrors
{
    public static Error OrderNotFound(string orderId) =>
        Error.NotFound("Order.NotFound", $"Order '{orderId}' was not found.");

    public static Error PalletNotFound(string orderId, string palletId) =>
        Error.NotFound("Pallet.NotFound", $"Pallet '{palletId}' was not found in order '{orderId}'.");

    public static Error CartonNotFound(string palletId, string cartonId) =>
        Error.NotFound("Carton.NotFound", $"Carton '{cartonId}' was not found on pallet '{palletId}'.");

    public static Error ProductLineNotFound(string orderId, int productLineId) =>
        Error.NotFound("ProductLine.NotFound", $"Product line '{productLineId}' was not found in order '{orderId}'.");
}
