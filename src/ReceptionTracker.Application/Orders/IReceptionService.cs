using ReceptionTracker.Application.Common;

namespace ReceptionTracker.Application.Orders;

/// <summary>
/// Reception use cases. Every update returns the whole refreshed order,
/// so the client gets the new statuses of all parents and the progress in one round trip.
/// </summary>
public interface IReceptionService
{
    Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(CancellationToken cancellationToken = default);

    Task<Result<OrderDto>> GetOrderAsync(string orderId, CancellationToken cancellationToken = default);

    Task<Result<OrderDto>> SetPalletReceivedAsync(
        string orderId, string palletId, bool isReceived, CancellationToken cancellationToken = default);

    Task<Result<OrderDto>> SetCartonReceivedAsync(
        string orderId, string palletId, string cartonId, bool isReceived, CancellationToken cancellationToken = default);

    Task<Result<OrderDto>> SetProductLineReceivedAsync(
        string orderId, int productLineId, bool isReceived, CancellationToken cancellationToken = default);
}
