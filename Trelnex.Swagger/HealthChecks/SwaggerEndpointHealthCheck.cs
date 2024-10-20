using Microsoft.Extensions.Diagnostics.HealthChecks;
using Trelnex.Swagger.Configuration;

namespace Trelnex.Swagger.HealthChecks;

/// <summary>
/// Initializes a new instance of the <see cref="SwaggerEndpointHealthCheck"/>.
/// </summary>
/// <param name="httpClientFactory">The specified <see cref="IHttpClientFactory"/> to create and configure an <see cref="HttpClient"/> instance.</param>
/// <param name="swaggerEndpoint">The <see cref="SwaggerEndpoint"/> for this health check.</param>
internal class SwaggerEndpointHealthCheck(
    IHttpClientFactory httpClientFactory,
    SwaggerEndpoint swaggerEndpoint) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, swaggerEndpoint.ProxyEndpoint!);
        var httpClient = httpClientFactory.CreateClient();
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken);

        var healthStatus = httpResponseMessage.IsSuccessStatusCode
            ? HealthStatus.Healthy
            : HealthStatus.Degraded;

        var data = new Dictionary<string, object>()
        {
            ["swaggerUrl"] = swaggerEndpoint.SwaggerURL!,
            ["proxyEndpoint"] = swaggerEndpoint.ProxyEndpoint!,
            ["statusCode"] = httpResponseMessage.StatusCode
        };

        return new HealthCheckResult(
            status: healthStatus,
            description: swaggerEndpoint.Name,
            data: data);
    }
}
