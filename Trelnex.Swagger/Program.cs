using AspNetCore.Proxy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Trelnex.Core.Api;
using Trelnex.Swagger.Configuration;
using Trelnex.Swagger.HealthChecks;

// secured swagger
// this works by exposing an external swagger url (say, "/swagger/petstore/v2/swagger.json")
// this external swagger url requires authorization:
//   authenticated user with role "swagger.read" is our swagger Azure app registration.
//   we support oidc/cookie (for web), and bearer auth (for curl or direct http)
// the external swagger url will proxy to an internal proxy endpoint (say, "https://petstore.swagger.io/v2/swagger.json")
// this swagger configuration ccomes from the SwaggerConfiguration config section

Application.Run(args, AddApplication, UseApplication, AddHealthChecks);

return;

void AddApplication(
    IServiceCollection services,
    IConfiguration configuration,
    ILogger bootstraplogger)
{
    // add oidc and cookie authentication schemes - this is for web
    // use the AzureAdOidc configuration section
    // set expiration for 12 hours
    services
        .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(authOptions =>
        {
            configuration.Bind("AzureAdOidc", authOptions);
            authOptions.MaxAge = TimeSpan.FromHours(12);
        }, sessionOptions =>
        {
            // https://brokul.dev/authentication-cookie-lifetime-and-sliding-expiration
            sessionOptions.ExpireTimeSpan = TimeSpan.FromHours(12);
            sessionOptions.Cookie.MaxAge = sessionOptions.ExpireTimeSpan;
            sessionOptions.SlidingExpiration = false;
        });

    // add bearer authentication scheme - this is for direct http to the json documents
    // use the AzureAdBearer configuration section
    services
        .AddAuthentication()
        .AddMicrosoftIdentityWebApi(configuration, "AzureAdBearer");

    // add the required services so we can proxy from local to remote
    // https://github.com/twitchax/AspNetCore.Proxy
    services.AddProxies();
}

void UseApplication(
    WebApplication app)
{
    // get the swagger endpoints
    var swaggerEndpoints = GetSwaggerEndpoints(app.Configuration);

    // log the swagger endpoints
    foreach (var swaggerEndpoint in swaggerEndpoints)
    {
        app.Logger.LogInformation("SwaggerEndpoint: {swaggerEndpoint}", swaggerEndpoint);
    }

    // add each swagger endpoint
    app.UseSwaggerUI(options =>
    {
        foreach (var swaggerEndpoint in swaggerEndpoints)
        {
            options.SwaggerEndpoint(swaggerEndpoint.SwaggerURL, swaggerEndpoint.Name);
        }
    });

    // create an authorization policy
    // bearer or oidc schemes
    // bearer must come first so it is evaluated first
    // otherwise, if bearer comes second and oidc fails and bearer fails, we have no opportunity to challenge
    var authenticationSchemes = new[] { JwtBearerDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme };

    var policy = new AuthorizationPolicyBuilder(authenticationSchemes)
        .RequireAuthenticatedUser()
        .RequireRole("swagger.read")
        .Build();

    // get the request delegate
    var requestDelegate = (app as IEndpointRouteBuilder).CreateApplicationBuilder().Build();

    // attach the authorization policy to the swagger ui routes
    app.Map("/swagger", requestDelegate).RequireAuthorization(policy);
    app.Map("/swagger/index.html", requestDelegate).RequireAuthorization(policy);

    // attach the authorization policy to the swagger endpoints
    foreach (var swaggerEndpoint in swaggerEndpoints)
    {
        app.Map(swaggerEndpoint.SwaggerURL, requestDelegate).RequireAuthorization(policy);
    }

    // this route will be hit if auth failed
    app.MapGet("/Account/AccessDenied", () => new { message = "access denied" });

    // map each local swagger url to the proxy endpoint
    // https://github.com/twitchax/AspNetCore.Proxy
    app.UseProxies(proxies =>
    {
        foreach (var swaggerEndpoint in swaggerEndpoints)
        {
            proxies.Map(swaggerEndpoint.SwaggerURL, proxy => proxy.UseHttp(swaggerEndpoint.ProxyEndpoint));
        }
    });
}

void AddHealthChecks(
    IHealthChecksBuilder builder,
    IConfiguration configuration)
{
    // get the swagger endpoints
    var swaggerEndpoints = GetSwaggerEndpoints(configuration);

    builder.AddSwaggerEndpointHealthChecks(swaggerEndpoints);
}

SwaggerEndpoint[] GetSwaggerEndpoints(
    IConfiguration configuration)
{
    // get the swagger configuration
    var swaggerConfiguration = configuration.GetSection("SwaggerConfiguration").Get<SwaggerConfiguration>();

    return swaggerConfiguration?.SwaggerEndpoints ?? [];
}
