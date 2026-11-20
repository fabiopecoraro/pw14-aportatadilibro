using System.Security.Claims;
using APortataDiLibro.API.Extensions;
using APortataDiLibro.Application.Features.Notifiche;

namespace APortataDiLibro.API.Endpoints;

public static class NotificheEndpoints
{
    public static IEndpointRouteBuilder MapNotificheEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/libri/{id:guid}/sottoscrizioni", async (Guid id, ClaimsPrincipal user, ISottoscrizioniService sottoscrizioniService, CancellationToken cancellationToken) =>
        {
            var sottoscrizione = await sottoscrizioniService.SottoscriviAsync(user.GetUserId(), id, cancellationToken);
            return Results.Created($"/api/sottoscrizioni/{sottoscrizione.Id}", sottoscrizione);
        })
        .WithTags("Notifiche")
        .WithName("SottoscriviDisponibilitaLibro")
        .RequireAuthorization();

        app.MapDelete("/api/libri/{id:guid}/sottoscrizioni", async (Guid id, ClaimsPrincipal user, ISottoscrizioniService sottoscrizioniService, CancellationToken cancellationToken) =>
        {
            await sottoscrizioniService.AnnullaSottoscrizioneAsync(user.GetUserId(), id, cancellationToken);
            return Results.NoContent();
        })
        .WithTags("Notifiche")
        .WithName("AnnullaSottoscrizioneDisponibilitaLibro")
        .RequireAuthorization();

        app.MapGet("/api/sottoscrizioni", async (ClaimsPrincipal user, ISottoscrizioniService sottoscrizioniService, CancellationToken cancellationToken) =>
        {
            var sottoscrizioni = await sottoscrizioniService.GetSottoscrizioniUtenteAsync(user.GetUserId(), cancellationToken);
            return Results.Ok(sottoscrizioni);
        })
        .WithTags("Notifiche")
        .WithName("GetSottoscrizioniUtente")
        .RequireAuthorization();

        return app;
    }
}
