using System.Security.Claims;
using APortataDiLibro.API.Extensions;
using APortataDiLibro.Application.Features.Chat;
using APortataDiLibro.Application.Features.Prestiti;
using APortataDiLibro.Application.Features.Prestiti.Dtos;

namespace APortataDiLibro.API.Endpoints;

public static class PrestitiEndpoints
{
    public static IEndpointRouteBuilder MapPrestitiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/copie/{copiaId:guid}/prestiti", async (Guid copiaId, RichiediPrestitoRequest request, ClaimsPrincipal user, IPrestitoService prestitoService, CancellationToken cancellationToken) =>
        {
            var prestito = await prestitoService.RichiediPrestitoAsync(copiaId, user.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/prestiti/{prestito.Id}", prestito);
        })
        .WithTags("Prestiti")
        .WithName("RichiediPrestito")
        .RequireAuthorization();

        var group = app.MapGroup("/api/prestiti").WithTags("Prestiti").RequireAuthorization();

        group.MapGet("/inviati", async (ClaimsPrincipal user, IPrestitoService prestitoService, CancellationToken cancellationToken) =>
        {
            var prestiti = await prestitoService.GetPrestitiInviatiAsync(user.GetUserId(), cancellationToken);
            return Results.Ok(prestiti);
        })
        .WithName("GetPrestitiInviati");

        group.MapGet("/ricevuti", async (ClaimsPrincipal user, IPrestitoService prestitoService, CancellationToken cancellationToken) =>
        {
            var prestiti = await prestitoService.GetPrestitiRicevutiAsync(user.GetUserId(), cancellationToken);
            return Results.Ok(prestiti);
        })
        .WithName("GetPrestitiRicevuti");

        group.MapPost("/{id:guid}/accetta", async (Guid id, ClaimsPrincipal user, IPrestitoService prestitoService, CancellationToken cancellationToken) =>
        {
            var prestito = await prestitoService.AccettaPrestitoAsync(id, user.GetUserId(), cancellationToken);
            return Results.Ok(prestito);
        })
        .WithName("AccettaPrestito");

        group.MapPost("/{id:guid}/rifiuta", async (Guid id, ClaimsPrincipal user, IPrestitoService prestitoService, CancellationToken cancellationToken) =>
        {
            var prestito = await prestitoService.RifiutaPrestitoAsync(id, user.GetUserId(), cancellationToken);
            return Results.Ok(prestito);
        })
        .WithName("RifiutaPrestito");

        group.MapPost("/{id:guid}/annulla", async (Guid id, ClaimsPrincipal user, IPrestitoService prestitoService, CancellationToken cancellationToken) =>
        {
            var prestito = await prestitoService.AnnullaPrestitoAsync(id, user.GetUserId(), cancellationToken);
            return Results.Ok(prestito);
        })
        .WithName("AnnullaPrestito");

        group.MapPost("/{id:guid}/completa", async (Guid id, ClaimsPrincipal user, IPrestitoService prestitoService, CancellationToken cancellationToken) =>
        {
            var prestito = await prestitoService.CompletaPrestitoAsync(id, user.GetUserId(), cancellationToken);
            return Results.Ok(prestito);
        })
        .WithName("CompletaPrestito");

        // Cronologia della chat privata del prestito: disponibile fin dalla
        // creazione della richiesta per i suoi due partecipanti.
        group.MapGet("/{id:guid}/messaggi", async (Guid id, ClaimsPrincipal user, IChatService chatService, CancellationToken cancellationToken) =>
        {
            var messaggi = await chatService.GetCronologiaAsync(id, user.GetUserId(), cancellationToken);
            return Results.Ok(messaggi);
        })
        .WithName("GetCronologiaChatPrestito");

        return app;
    }
}
