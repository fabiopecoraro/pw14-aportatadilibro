using APortataDiLibro.Application.Features.Ricerca;

namespace APortataDiLibro.API.Endpoints;

public static class RicercaEndpoints
{
    public static IEndpointRouteBuilder MapRicercaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ricerca").WithTags("Ricerca");

        // Il bounding box corrisponde all'area di mappa visibile sul frontend
        // e viene usato solo per questa query: non e' mai persistito su database.
        group.MapGet("/libri", async (
            double sud,
            double ovest,
            double nord,
            double est,
            string? titolo,
            string? autore,
            Guid? categoriaId,
            IRicercaService ricercaService,
            CancellationToken cancellationToken) =>
        {
            var risultati = await ricercaService.CercaLibriAsync(sud, ovest, nord, est, titolo, autore, categoriaId, cancellationToken);
            return Results.Ok(risultati);
        })
        .WithName("CercaLibriPerArea")
        .AllowAnonymous();

        return app;
    }
}
