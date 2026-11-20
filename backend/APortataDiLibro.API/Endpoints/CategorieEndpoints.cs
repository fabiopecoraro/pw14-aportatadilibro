using APortataDiLibro.Application.Features.Categorie;
using APortataDiLibro.Application.Features.Categorie.Dtos;
using APortataDiLibro.Core.Constants;

namespace APortataDiLibro.API.Endpoints;

public static class CategorieEndpoints
{
    public static IEndpointRouteBuilder MapCategorieEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categorie").WithTags("Categorie");

        // La lettura del vocabolario e' aperta: serve ai form di inserimento,
        // ai filtri del catalogo e della mappa. Le scritture no — il
        // vocabolario e' un metadato condiviso da tutto il catalogo, quindi
        // segue la stessa regola dei metadati core dei libri.
        group.MapGet("/", async (ICategorieService categorieService, CancellationToken cancellationToken) =>
        {
            var categorie = await categorieService.GetTutteAsync(cancellationToken);
            return Results.Ok(categorie);
        })
        .WithName("GetCategorie")
        .AllowAnonymous();

        group.MapGet("/gestione", async (ICategorieService categorieService, CancellationToken cancellationToken) =>
        {
            var categorie = await categorieService.GetPerGestioneAsync(cancellationToken);
            return Results.Ok(categorie);
        })
        .WithName("GetCategoriePerGestione")
        .RequireAuthorization(AuthConstants.AdminPolicy);

        group.MapPost("/", async (SalvaCategoriaRequest request, ICategorieService categorieService, CancellationToken cancellationToken) =>
        {
            var categoria = await categorieService.CreaAsync(request, cancellationToken);
            return Results.Created($"/api/categorie/{categoria.Id}", categoria);
        })
        .WithName("CreaCategoria")
        .RequireAuthorization(AuthConstants.AdminPolicy);

        group.MapPut("/{id:guid}", async (Guid id, SalvaCategoriaRequest request, ICategorieService categorieService, CancellationToken cancellationToken) =>
        {
            var categoria = await categorieService.RinominaAsync(id, request, cancellationToken);
            return Results.Ok(categoria);
        })
        .WithName("RinominaCategoria")
        .RequireAuthorization(AuthConstants.AdminPolicy);

        group.MapDelete("/{id:guid}", async (Guid id, ICategorieService categorieService, CancellationToken cancellationToken) =>
        {
            await categorieService.EliminaAsync(id, cancellationToken);
            return Results.NoContent();
        })
        .WithName("EliminaCategoria")
        .RequireAuthorization(AuthConstants.AdminPolicy);

        return app;
    }
}
