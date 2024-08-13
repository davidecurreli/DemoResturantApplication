using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace ApplicationGateway.Providers;

// Yarp request transformer. It translates the incoming request to the dapr invokation template to connect to the underlining microservice
// PATCH/POST/GET/PUT/DELETE http://localhost:<daprPort=3500>/v1.0/invoke/<appID>/method/<method-name>
public class DaprTransformProvider : ITransformProvider
{
    public void Apply(TransformBuilderContext context)
    {
        var daprAppId = string.Empty;
        context.Route.Metadata?.TryGetValue("DaprAppId", out daprAppId);

        if (daprAppId is null || string.IsNullOrEmpty(daprAppId))
            throw new ArgumentException("A valid Dapr AppId value is required");

        context.AddRequestTransform(transformContext =>
        {
            transformContext.ProxyRequest.Headers.Add("dapr-app-id", daprAppId);
            transformContext.ProxyRequest.RequestUri = new Uri($"{transformContext.DestinationPrefix}{transformContext.Path.Value!}{transformContext.Query.QueryString.Value}");
            return ValueTask.CompletedTask;
        });
    }

    public void ValidateCluster(TransformClusterValidationContext context) { return; }

    public void ValidateRoute(TransformRouteValidationContext context) { return; }
}