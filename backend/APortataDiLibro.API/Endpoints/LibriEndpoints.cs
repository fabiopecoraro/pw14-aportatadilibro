using System.Security.Claims;
using APortataDiLibro.API.Extensions;
using APortataDiLibro.Application.Features.Catalogo;
using APortataDiLibro.Application.Features.Catalogo.Dtos;
using APortataDiLibro.Core.Constants;

namespace APortataDiLibro.API.Endpoints;

public static class LibriEndpoints
{
    public static IEndpointRouteBuilder MapLibriEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/libri").WithTags("Catalogo");

        group.MapGet("/", async (string? titolo, string? autore, Guid? categoriaId, ILibroService libroService, CancellationToken cancellationToken) =>
        {
            var risultati = await libroService.CercaLibriAsync(titolo, autore, categoriaId, cancellationToken);
            return Results.Ok(risultati);
        })
        .WithName("CercaLibri")
        .AllowAnonymous();

        group.MapGet("/{id:guid}", async (Guid id, ILibroService libroService, CancellationToken cancellationToken) =>
        {
            var dettaglio = await libroService.GetDettaglioAsync(id, cancellationToken);
            return Results.Ok(dettaglio);
        })
        .WithName("GetDettaglioLibro")
        .AllowAnonymous();

        group.MapGet("/mie-copie", async (ClaimsPrincipal user, ILibroService libroService, CancellationToken cancellationToken) =>
        {
            var libreria = await libroService.GetLibreriaUtenteAsync(user.GetUserId(), cancellationToken);
            return Results.Ok(libreria);
        })
        .WithName("GetLibreriaUtente")
        .RequireAuthorization();

        group.MapPost("/", async (CreaLibroRequest request, ILibroService libroService, CancellationToken cancellationToken) =>
        {
            var libro = await libroService.CreaLibroAsync(request, cancellationToken);
            return Results.Created($"/api/libri/{libro.Id}", libro);
        })
        .WithName("CreaLibro")
        // La creazione diretta e' dell'Admin: un utente che vuole un'opera
        // nuova inoltra una proposta. Lasciando il percorso aperto a chiunque
        // sia autenticato, il catalogo condiviso si potrebbe riempire senza
        // che nessuno lo abbia approvato.
        .RequireAuthorization(AuthConstants.AdminPolicy);

        group.MapPut("/{id:guid}", async (Guid id, AggiornaLibroRequest request, ILibroService libroService, CancellationToken cancellationToken) =>
        {
            var libro = await libroService.AggiornaMetadatiAsync(id, request, cancellationToken);
            return Results.Ok(libro);
        })
        .WithName("AggiornaLibro")
        .RequireAuthorization(AuthConstants.AdminPolicy);

        group.MapPost("/{id:guid}/copie", async (Guid id, ClaimsPrincipal user, ILibroService libroService, CancellationToken cancellationToken) =>
        {
            var copia = await libroService.AggiungiCopiaAsync(id, user.GetUserId(), cancellationToken);
            return Results.Created($"/api/libri/{id}", copia);
        })
        .WithName("AggiungiCopiaLibro")
        .RequireAuthorization();

        app.MapDelete("/api/copie/{id:guid}", async (Guid id, ClaimsPrincipal user, ILibroService libroService, CancellationToken cancellationToken) =>
        {
            await libroService.RimuoviCopiaAsync(id, user.GetUserId(), cancellationToken);
            return Results.NoContent();
        })
        .WithTags("Catalogo")
        .WithName("RimuoviCopiaLibro")
        .RequireAuthorization();

        app.MapPost("/api/copie/{id:guid}/copertina", async (Guid id, IFormFile file, ClaimsPrincipal user, ILibroService libroService, CancellationToken cancellationToken) =>
        {
            await using var stream = file.OpenReadStream();
            var copia = await libroService.CaricaCopertinaAsync(id, user.GetUserId(), stream, cancellationToken);
            return Results.Ok(copia);
        })
        .WithTags("Catalogo")
        .WithName("CaricaCopertinaCopia")
        .RequireAuthorization()
        .DisableAntiforgery();

        return app;
    }
}
