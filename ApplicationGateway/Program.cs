using ApplicationGateway.Extensions;

string appName = typeof(Program).Assembly.GetName().Name ?? "ApplicationGateway";

var builder = WebApplication.CreateBuilder(args);

builder.AddReverseProxy(builder.Configuration);

var app = builder.Build();

app.UseReverseProxy();

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
//finally
//{
//   Logger.CloseAndFlush();
//}
