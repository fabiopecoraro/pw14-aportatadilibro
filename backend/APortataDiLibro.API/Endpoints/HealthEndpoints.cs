namespace APortataDiLibro.API.Endpoints;

/// <summary>
/// Endpoint di servizio per verificare la disponibilita' dell'API.
/// Ha la stessa forma di tutti i gruppi di endpoint del dominio: un metodo
/// di estensione su <see cref="IEndpointRouteBuilder"/>, cosi' Program.cs li
/// registra con una riga ciascuno e non contiene nessuna rotta.
/// </summary>
public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("GetHealth")
            .WithTags("Health");

        return app;
    }
}
