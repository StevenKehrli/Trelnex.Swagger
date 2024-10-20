using Microsoft.Extensions.Diagnostics.HealthChecks;
using Trelnex.Swagger.Configuration;

namespace Trelnex.Swagger.HealthChecks;

/// <summary>
/// Extension methods to add the Swagger endpoint health checks to the <see cref="IHealthChecksBuilder"/>.
/// </summary>
public static class SwaggerEndpointHealthCheckExtensions
{
    /// <summary>
    /// Add the Swagger endpoint health checks to the <see cref="IHealthChecksBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the additional health checks to.</param>
    /// <param name="swaggerEndpoints">The collection of <see cref="SwaggerEndpoint"/> the specify the Swagger endpoints.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
    public static IHealthChecksBuilder AddSwaggerEndpointHealthChecks(
        this IHealthChecksBuilder builder,
        SwaggerEndpoint[] swaggerEndpoints)
    {
        // add the Swagger endpoint health checks
        Array.ForEach(swaggerEndpoints, swaggerEndpoint =>
        {
            var healthCheckName = $"SwaggerEndpointHealthCheck: {swaggerEndpoint.Name}";

            builder.Add(
                new HealthCheckRegistration(
                    name: healthCheckName,
                    factory: services =>
                    {
                        var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();

                        return new SwaggerEndpointHealthCheck(httpClientFactory, swaggerEndpoint);
                    },
                    failureStatus: null,
                    tags: null));
        });

        return builder;
    }
}
