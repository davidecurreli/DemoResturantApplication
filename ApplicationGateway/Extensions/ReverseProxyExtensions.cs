using ApplicationGateway.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ApplicationGateway.Providers;
using ApplicationGateway.Helpers;

namespace ApplicationGateway.Extensions;
public static class ReverseProxyExtensions
{
    public static void AddReverseProxy(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        // Yarp DI
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
            .AddTransforms<DaprTransformProvider>();

        // Authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = RsaSecurityHelper.CreateRsaSecurityKey(),
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuer = true,
                    RequireSignedTokens = true,
                    ValidIssuer = configuration.GetValue<string>("Authorization:ValidIssuer")
                };
            });

        // Authorization
        builder.Services.AddAuthorization();
        // Dapr DI
        builder.Services.AddDaprClient();

        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(policyName: "standardPolicy", options =>
            {
                options.PermitLimit = 200;
                options.Window = TimeSpan.FromSeconds(10);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 100;
            });
        });
    }

    public static void UseReverseProxy(this WebApplication app)
    {
        app.UseRouting();
        app.MapReverseProxy();
        app.UseRateLimiter();
        app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
        );
        app.UseAuthentication();
        app.UseAuthorization();
    }
}