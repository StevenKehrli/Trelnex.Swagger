# Trelnex.Swagger

`Trelnex.Swagger` adds authentication and authorization to the Swagger UI routes (`/swagger` and `/swagger/index.html`) and the Swagger endpoints (OpenAPI documents).

This is intended as the publicly facing Swagger UI and endpoints while the sensitive OpenAPI documents remain private. This would be appropriate in a Kubernetes environment where `Trelnex.Swagger` is the exposed public ingress but the sensitive OpenAPI documents are private within the cluster.

## Authentication and Authorization

These samples use Microsoft Identity for authentication and authorization.

See [https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app?tabs=certificate](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app?tabs=certificate) for more information.

appsettings.json must be configured with the valid Instance, TenantId, ClientId, and Audience. Audience is the Application ID URI for the app registration.

## AspNetCore.Proxy

`Trelnex.Swagger` uses [AspNetCore.Proxy](https://github.com/twitchax/AspNetCore.Proxy) to proxy an authenticated and authorized request to the correct endpoint (OpenAPI document).

## Endpoints

The Swagger endpoints are configured in appsettings.json:

```json
  "SwaggerConfiguration": {
    "SwaggerEndpoints": [
      {
        "Name": "PetStore v2",
        "SwaggerUrl": "/petstore/v2/swagger.json",
        "ProxyEndpoint": "https://petstore.swagger.io/v2/swagger.json"
      }
    ]
  }
```

`Name` is the description that appears in the Swagger UI document selector drop-down.

`SwaggerURL` is the relative endpoint to the Swagger json file. This is the public endpoint.

`ProxyEndpoint` is the proxy endpoint to the Swagger json file. This is the private endpoint.

In a Kubernetes environment, `ProxyEndpoint` would look something like `http://petstore.swagger.svc.cluster.local:8080/swagger/v2/swagger.json`.
