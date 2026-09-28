using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ReceptionTracker.Api.Tests.Infrastructure;
using ReceptionTracker.Application.Orders;
using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Api.Tests.Orders;

public sealed class GetOrdersTests : ApiTestBase
{
    [Fact]
    public async Task List_returns_the_seeded_orders_with_their_progress()
    {
        var orders = await Client.GetFromJsonAsync<List<OrderSummaryDto>>("/api/orders", JsonOptions);

        Assert.NotNull(orders);
        Assert.Collection(orders,
            o =>
            {
                Assert.Equal("CMD-2026", o.OrderId);
                Assert.Equal(ReceptionStatus.Pending, o.Status);
                Assert.Equal(new ProgressDto(0, 9, 0, 255), o.Progress);
            },
            o => Assert.Equal("CMD-2027", o.OrderId));
    }

    [Fact]
    public async Task Detail_returns_the_full_hierarchy_in_a_stable_order()
    {
        var order = await GetOrderAsync("CMD-2026");

        Assert.Equal(ReceptionStatus.Pending, order.Status);
        Assert.Equal(["PAL-01", "PAL-02"], order.Pallets.Select(p => p.PalletId));
        Assert.Equal(["CART-01-A", "CART-01-B"], Pallet(order, "PAL-01").Cartons.Select(c => c.CartonId));

        var product = Carton(order, "PAL-01", "CART-01-A").Products[0];
        Assert.Equal("TSH-RED-M", product.Ref);
        Assert.Equal("T-Shirt Sport", product.Name);
        Assert.Equal("Rouge", product.Color);
        Assert.Equal("M", product.Size);
        Assert.Equal(50, product.ExpectedQuantity);
        Assert.False(product.IsReceived);
    }

    [Fact]
    public async Task Detail_serializes_statuses_as_strings()
    {
        var json = await Client.GetFromJsonAsync<JsonElement>("/api/orders/CMD-2026");

        Assert.Equal("Pending", json.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Unknown_order_returns_404_problem_details()
    {
        var response = await Client.GetAsync("/api/orders/CMD-9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Order.NotFound", problem.GetProperty("code").GetString());
    }
}
