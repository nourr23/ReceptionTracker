using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReceptionTracker.Application.Orders;

namespace ReceptionTracker.Api.Tests.Infrastructure;

/// <summary>
/// xUnit creates a new test class instance per test, so each test gets
/// its own API and a freshly seeded database: tests never affect each other.
/// </summary>
public abstract class ApiTestBase : IAsyncLifetime
{
    // Same JSON conventions as the API: camelCase and enums as strings.
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ReceptionApiFactory _factory = new();

    protected HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
    }

    protected async Task<OrderDto> GetOrderAsync(string orderId)
    {
        var order = await Client.GetFromJsonAsync<OrderDto>($"/api/orders/{orderId}", JsonOptions);
        return order!;
    }

    protected Task<HttpResponseMessage> PutReceptionAsync(string url, bool isReceived) =>
        Client.PutAsJsonAsync(url, new { isReceived }, JsonOptions);

    protected static async Task<OrderDto> ReadOrderAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var order = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        return order!;
    }

    protected static PalletDto Pallet(OrderDto order, string palletId) =>
        order.Pallets.Single(p => p.PalletId == palletId);

    protected static CartonDto Carton(OrderDto order, string palletId, string cartonId) =>
        Pallet(order, palletId).Cartons.Single(c => c.CartonId == cartonId);
}
