namespace Trelnex.Swagger.Configuration;

/// <summary>
/// Represents the configuration properties for Swagger.
/// </summary>
/// <remarks>
/// <para>
/// https://github.com/dotnet/runtime/issues/83803
/// </para>
/// </remarks>
public class SwaggerConfiguration
{
    /// <summary>
    /// The collection of Swagger json endpoints (description, local endpoint, proxy endpoint).
    /// </summary>
    public SwaggerEndpoint[]? SwaggerEndpoints { get; set; }
}
