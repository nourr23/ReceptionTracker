using Microsoft.AspNetCore.Http.HttpResults;
using ReceptionTracker.Api.Common;
using ReceptionTracker.Application.Orders;

namespace ReceptionTracker.Api.Orders;

internal static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/api/orders").WithTags("Orders");

        orders.MapGet("/", GetOrders)
            .WithName(nameof(GetOrders))
            .WithSummary("List the orders with their reception status and progress.");

        orders.MapGet("/{orderId}", GetOrder)
            .WithName(nameof(GetOrder))
            .WithSummary("Get an order with its full hierarchy: pallets, cartons and product lines.");

        orders.MapPut("/{orderId}/pallets/{palletId}/reception", SetPalletReception)
            .WithName(nameof(SetPalletReception))
            .WithSummary("Validate or un-validate a pallet, with all its cartons and products.");

        orders.MapPut("/{orderId}/pallets/{palletId}/cartons/{cartonId}/reception", SetCartonReception)
            .WithName(nameof(SetCartonReception))
            .WithSummary("Validate or un-validate a carton, with all its products.");

        orders.MapPut("/{orderId}/products/{productLineId:int}/reception", SetProductLineReception)
            .WithName(nameof(SetProductLineReception))
            .WithSummary("Validate or un-validate a single product line.");

        return app;
    }

    private static async Task<Ok<IReadOnlyList<OrderSummaryDto>>> GetOrders(
        IReceptionService service, CancellationToken cancellationToken) =>
        TypedResults.Ok(await service.GetOrdersAsync(cancellationToken));

    private static async Task<Results<Ok<OrderDto>, ProblemHttpResult>> GetOrder(
        string orderId, IReceptionService service, CancellationToken cancellationToken) =>
        (await service.GetOrderAsync(orderId, cancellationToken)).ToOkOrProblem();

    private static async Task<Results<Ok<OrderDto>, ProblemHttpResult>> SetPalletReception(
        string orderId, string palletId, SetReceptionRequest request,
        IReceptionService service, CancellationToken cancellationToken) =>
        (await service.SetPalletReceivedAsync(orderId, palletId, request.IsReceived, cancellationToken))
            .ToOkOrProblem();

    private static async Task<Results<Ok<OrderDto>, ProblemHttpResult>> SetCartonReception(
        string orderId, string palletId, string cartonId, SetReceptionRequest request,
        IReceptionService service, CancellationToken cancellationToken) =>
        (await service.SetCartonReceivedAsync(orderId, palletId, cartonId, request.IsReceived, cancellationToken))
            .ToOkOrProblem();

    private static async Task<Results<Ok<OrderDto>, ProblemHttpResult>> SetProductLineReception(
        string orderId, int productLineId, SetReceptionRequest request,
        IReceptionService service, CancellationToken cancellationToken) =>
        (await service.SetProductLineReceivedAsync(orderId, productLineId, request.IsReceived, cancellationToken))
            .ToOkOrProblem();
}
