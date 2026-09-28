using ReceptionTracker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Creates/updates the SQLite database and seeds the fake orders.
    await app.Services.MigrateDatabaseAsync();
}

app.UseHttpsRedirection();

await app.RunAsync();

// Exposes the Program class to the integration tests (WebApplicationFactory<Program>).
public partial class Program;
