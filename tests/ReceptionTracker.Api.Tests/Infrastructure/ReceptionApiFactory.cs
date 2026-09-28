using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using ReceptionTracker.Infrastructure;

namespace ReceptionTracker.Api.Tests.Infrastructure;

/// <summary>
/// Runs the real API in memory against its own in-memory SQLite database,
/// migrated and seeded, so every factory instance starts from the same known data.
/// </summary>
public sealed class ReceptionApiFactory : WebApplicationFactory<Program>
{
    // A named, shared-cache in-memory database: every connection using this name sees the same data.
    private readonly string _connectionString =
        $"Data Source=reception-tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

    // An in-memory SQLite database is deleted when its last connection closes: this one keeps it alive.
    private readonly SqliteConnection _keepAliveConnection;

    public ReceptionApiFactory()
    {
        _keepAliveConnection = new SqliteConnection(_connectionString);
    }

    public async Task InitializeDatabaseAsync()
    {
        await _keepAliveConnection.OpenAsync();
        await Services.MigrateDatabaseAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not "Development": the tests must not depend on the dev-only startup migration or API docs.
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ReceptionDb"] = _connectionString
            }));
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _keepAliveConnection.DisposeAsync();
    }
}
