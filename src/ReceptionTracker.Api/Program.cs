using System.Text.Json.Serialization;
using ReceptionTracker.Api.Common;
using ReceptionTracker.Api.Orders;
using ReceptionTracker.Application;
using ReceptionTracker.Infrastructure;
using Scalar.AspNetCore;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ClientErrorExceptionHandler>();

// Enums are sent as "PartiallyReceived" instead of 1: readable and stable for the front-end.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
app.UseCors(FrontendCorsPolicy);

app.MapOrderEndpoints();

await app.RunAsync();

// Exposes the Program class to the integration tests (WebApplicationFactory<Program>).
public partial class Program;
