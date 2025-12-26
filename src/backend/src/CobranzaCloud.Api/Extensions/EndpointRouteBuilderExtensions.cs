using CobranzaCloud.Api.Endpoints;

namespace CobranzaCloud.Api.Extensions;

/// <summary>
/// Extension methods for IEndpointRouteBuilder to map all API endpoints
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        // System endpoints (health, info)
        app.MapSystemEndpoints();

        // Auth endpoints (M1)
        app.MapAuthEndpoints();

        // Connector and Sync endpoints (M2)
        app.MapConnectorsEndpoints();
        app.MapSyncEndpoints();

        // Cartera and Clientes endpoints (M3)
        app.MapCarteraEndpoints();
        app.MapClientesEndpoints();

        // Future endpoints will be added here:
        // app.MapCobranzaEndpoints();

        return app;
    }
}
