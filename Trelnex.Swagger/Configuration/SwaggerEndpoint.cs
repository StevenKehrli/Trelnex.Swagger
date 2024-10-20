namespace Trelnex.Swagger.Configuration;

/// <summary>
/// Represents an endpoint for a Swagger json file.
/// </summary>
/// <param name="Name">The description that appears in the document selector drop-down.</param>
/// <param name="SwaggerURL">The relative endpoint to the Swagger json file.</param>
/// <param name="ProxyEndpoint">The proxy endpoint to the Swagger json file.</param>
public record SwaggerEndpoint(
    string Name,
    string SwaggerURL,
    string ProxyEndpoint);
