using System.Text.Json.Serialization;
using ReceptionTracker.Api.Common;
using ReceptionTracker.Api.Orders;
using ReceptionTracker.Application;
using ReceptionTracker.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ClientErrorExceptionHandler>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Enums are sent as "PartiallyReceived" instead of 1: readable and stable for the front-end.
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    // Numbers must be JSON numbers ("42" is rejected), which also keeps the OpenAPI types precise.
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Creates/updates the SQLite database and seeds the fake orders.
    await app.Services.MigrateDatabaseAsync();
}

app.UseHttpsRedirection();

app.MapOrderEndpoints();

await app.RunAsync();

// Exposes the Program class to the integration tests (WebApplicationFactory<Program>).
public partial class Program;
