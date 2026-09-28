using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ReceptionTracker.Api.Tests.Infrastructure;
using ReceptionTracker.Application.Orders;
using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Api.Tests.Orders;

// Seeded order CMD-2026 (see OrderSeeder):
// PAL-01 ─┬─ CART-01-A: TSH-RED-M (50), SHO-BLK-42 (10)
//         └─ CART-01-B: TSH-RED-L (40), TSH-BLU-M (30)
// PAL-02 ─┬─ CART-02-A: SHO-BLK-43 (12), SHO-WHT-41 (8)
//         ├─ CART-02-B: BAL-FOOT-5 (20), SRT-BLK-M (25)
//         └─ CART-02-C: GRD-BLK-TU (60)
public sealed class ReceptionTests : ApiTestBase
{
    private const string OrderUrl = "/api/orders/CMD-2026";

    [Fact]
    public async Task Validating_a_carton_receives_its_products_and_makes_the_pallet_partial()
    {
        var response = await PutReceptionAsync($"{OrderUrl}/pallets/PAL-01/cartons/CART-01-A/reception", true);
        var order = await ReadOrderAsync(response);

        var carton = Carton(order, "PAL-01", "CART-01-A");
        Assert.Equal(ReceptionStatus.Received, carton.Status);
        Assert.All(carton.Products, p => Assert.True(p.IsReceived));
        Assert.Equal(ReceptionStatus.Pending, Carton(order, "PAL-01", "CART-01-B").Status);
        Assert.Equal(ReceptionStatus.PartiallyReceived, Pallet(order, "PAL-01").Status);
        Assert.Equal(new ProgressDto(2, 9, 60, 255), order.Progress);
    }

    [Fact]
    public async Task Validating_a_pallet_receives_all_its_cartons()
    {
        var response = await PutReceptionAsync($"{OrderUrl}/pallets/PAL-02/reception", true);
        var order = await ReadOrderAsync(response);

        Assert.Equal(ReceptionStatus.Received, Pallet(order, "PAL-02").Status);
        Assert.All(Pallet(order, "PAL-02").Cartons, c => Assert.Equal(ReceptionStatus.Received, c.Status));
        Assert.Equal(ReceptionStatus.Pending, Pallet(order, "PAL-01").Status);
    }

    [Fact]
    public async Task Validating_every_product_one_by_one_marks_the_carton_as_received()
    {
        var products = Carton(await GetOrderAsync("CMD-2026"), "PAL-01", "CART-01-A").Products;

        OrderDto order = null!;
        foreach (var product in products)
        {
            order = await ReadOrderAsync(await PutReceptionAsync($"{OrderUrl}/products/{product.Id}/reception", true));
        }

        Assert.Equal(ReceptionStatus.Received, Carton(order, "PAL-01", "CART-01-A").Status);
    }

    [Fact]
    public async Task Unchecking_a_product_puts_its_carton_and_pallet_back_to_partial()
    {
        await PutReceptionAsync($"{OrderUrl}/pallets/PAL-01/reception", true);
        var productId = Carton(await GetOrderAsync("CMD-2026"), "PAL-01", "CART-01-A").Products[0].Id;

        var order = await ReadOrderAsync(await PutReceptionAsync($"{OrderUrl}/products/{productId}/reception", false));

        Assert.Equal(ReceptionStatus.PartiallyReceived, Carton(order, "PAL-01", "CART-01-A").Status);
        Assert.Equal(ReceptionStatus.Received, Carton(order, "PAL-01", "CART-01-B").Status);
        Assert.Equal(ReceptionStatus.PartiallyReceived, Pallet(order, "PAL-01").Status);
    }

    [Fact]
    public async Task Validating_every_pallet_marks_the_order_as_received()
    {
        await PutReceptionAsync($"{OrderUrl}/pallets/PAL-01/reception", true);
        var order = await ReadOrderAsync(await PutReceptionAsync($"{OrderUrl}/pallets/PAL-02/reception", true));

        Assert.Equal(ReceptionStatus.Received, order.Status);
        Assert.Equal(new ProgressDto(9, 9, 255, 255), order.Progress);
    }

    [Fact]
    public async Task Changes_are_persisted()
    {
        await PutReceptionAsync($"{OrderUrl}/pallets/PAL-01/cartons/CART-01-B/reception", true);

        var order = await GetOrderAsync("CMD-2026");

        Assert.Equal(ReceptionStatus.Received, Carton(order, "PAL-01", "CART-01-B").Status);
    }

    [Fact]
    public async Task Validating_twice_is_idempotent()
    {
        var first = await ReadOrderAsync(await PutReceptionAsync($"{OrderUrl}/pallets/PAL-01/reception", true));
        var second = await ReadOrderAsync(await PutReceptionAsync($"{OrderUrl}/pallets/PAL-01/reception", true));

        Assert.Equal(first.Progress, second.Progress);
        Assert.Equal(ReceptionStatus.Received, Pallet(second, "PAL-01").Status);
    }

    [Theory]
    [InlineData("/api/orders/CMD-9999/pallets/PAL-01/reception", "Order.NotFound")]
    [InlineData("/api/orders/CMD-2026/pallets/PAL-99/reception", "Pallet.NotFound")]
    [InlineData("/api/orders/CMD-2026/pallets/PAL-01/cartons/CART-02-A/reception", "Carton.NotFound")]
    [InlineData("/api/orders/CMD-2026/products/999999/reception", "ProductLine.NotFound")]
    public async Task Unknown_element_returns_404_with_an_error_code(string url, string expectedCode)
    {
        var response = await PutReceptionAsync(url, true);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(expectedCode, problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Product_of_another_order_cannot_be_validated_through_this_order()
    {
        var otherOrderProductId = Carton(await GetOrderAsync("CMD-2027"), "PAL-01", "CART-01-A").Products[0].Id;

        var response = await PutReceptionAsync($"{OrderUrl}/products/{otherOrderProductId}/reception", true);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"isReceived\":")]
    [InlineData("{\"isReceived\":\"yes\"}")]
    public async Task Invalid_body_returns_400_and_changes_nothing(string body)
    {
        await PutReceptionAsync($"{OrderUrl}/pallets/PAL-01/reception", true);

        var response = await Client.PutAsync($"{OrderUrl}/pallets/PAL-01/reception",
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ReceptionStatus.Received, Pallet(await GetOrderAsync("CMD-2026"), "PAL-01").Status);
    }
}
