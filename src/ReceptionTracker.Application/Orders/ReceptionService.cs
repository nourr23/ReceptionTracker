using ReceptionTracker.Application.Common;
using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Application.Orders;

internal sealed class ReceptionService(IOrderRepository orders, IUnitOfWork unitOfWork) : IReceptionService
{
    public async Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        var all = await orders.ListAsync(cancellationToken);
        return [.. all.Select(o => o.ToSummaryDto())];
    }

    public async Task<Result<OrderDto>> GetOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByReferenceAsync(orderId, cancellationToken);
        return order is null ? OrderErrors.OrderNotFound(orderId) : order.ToDto();
    }

    public Task<Result<OrderDto>> SetPalletReceivedAsync(
        string orderId, string palletId, bool isReceived, CancellationToken cancellationToken = default) =>
        UpdateOrderAsync(orderId, order =>
        {
            var pallet = order.FindPallet(palletId);
            if (pallet is null)
            {
                return OrderErrors.PalletNotFound(orderId, palletId);
            }

            pallet.SetReceived(isReceived);
            return null;
        }, cancellationToken);

    public Task<Result<OrderDto>> SetCartonReceivedAsync(
        string orderId, string palletId, string cartonId, bool isReceived, CancellationToken cancellationToken = default) =>
        UpdateOrderAsync(orderId, order =>
        {
            var pallet = order.FindPallet(palletId);
            if (pallet is null)
            {
                return OrderErrors.PalletNotFound(orderId, palletId);
            }

            var carton = pallet.FindCarton(cartonId);
            if (carton is null)
            {
                return OrderErrors.CartonNotFound(palletId, cartonId);
            }

            carton.SetReceived(isReceived);
            return null;
        }, cancellationToken);

    public Task<Result<OrderDto>> SetProductLineReceivedAsync(
        string orderId, int productLineId, bool isReceived, CancellationToken cancellationToken = default) =>
        UpdateOrderAsync(orderId, order =>
        {
            var productLine = order.FindProductLine(productLineId);
            if (productLine is null)
            {
                return OrderErrors.ProductLineNotFound(orderId, productLineId);
            }

            productLine.SetReceived(isReceived);
            return null;
        }, cancellationToken);

    /// <summary>
    /// Shared flow of every update: load the order, apply the change
    /// (which returns an error or null), save, and return the refreshed order.
    /// </summary>
    private async Task<Result<OrderDto>> UpdateOrderAsync(
        string orderId, Func<Order, Error?> applyChange, CancellationToken cancellationToken)
    {
        var order = await orders.GetByReferenceAsync(orderId, cancellationToken);
        if (order is null)
        {
            return OrderErrors.OrderNotFound(orderId);
        }

        var error = applyChange(order);
        if (error is not null)
        {
            return error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order.ToDto();
    }
}
