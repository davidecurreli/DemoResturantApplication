using Infrastructure;

string appName = typeof(Program).Assembly.GetName().Name ?? "OrdersMicroservice";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapControllers();

// Liveness Check required for daprSidecar
app.MapGet("/", Results.NoContent);
app.MapGet("/dapr/config", Results.NoContent);
app.MapGet("/dapr/subscribe", Results.NoContent);

try
{
    app.Logger.LogInformation("Starting API gateway ({ApplicationName})...", appName);
    app.Run();
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "API gateway terminated unexpectedly ({ApplicationName})...", appName);
}
