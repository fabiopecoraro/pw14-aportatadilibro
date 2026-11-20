using System.Security.Claims;
using APortataDiLibro.API.Extensions;
using APortataDiLibro.Application.Features.Hub;
using APortataDiLibro.Application.Features.Hub.Dtos;

namespace APortataDiLibro.API.Endpoints;

/// <summary>
/// Endpoint per la gestione degli Hub (punti di scambio pubblici) dell'utente
/// e per le esclusioni Copia/Hub. Da non confondere con <c>app.MapHub&lt;T&gt;</c>
/// di SignalR, usato per il chat hub su <c>/hubs/chat</c>.
/// </summary>
public static class HubEndpoints
{
    public static IEndpointRouteBuilder MapHubEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/hub").WithTags("Hub").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IHubService hubService, CancellationToken cancellationToken) =>
        {
            var hub = await hubService.GetHubUtenteAsync(user.GetUserId(), cancellationToken);
            return Results.Ok(hub);
        })
        .WithName("GetHubUtente");

        group.MapPost("/", async (CreaHubRequest request, ClaimsPrincipal user, IHubService hubService, CancellationToken cancellationToken) =>
        {
            var hub = await hubService.CreaHubAsync(user.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/hub/{hub.Id}", hub);
        })
        .WithName("CreaHub");

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal user, IHubService hubService, CancellationToken cancellationToken) =>
        {
            await hubService.EliminaHubAsync(id, user.GetUserId(), cancellationToken);
            return Results.NoContent();
        })
        .WithName("EliminaHub");

        app.MapGet("/api/copie/{copiaId:guid}/esclusioni", async (Guid copiaId, ClaimsPrincipal user, IHubService hubService, CancellationToken cancellationToken) =>
        {
            var disponibilita = await hubService.GetDisponibilitaCopiaAsync(copiaId, user.GetUserId(), cancellationToken);
            return Results.Ok(disponibilita);
        })
        .WithTags("Hub")
        .WithName("GetDisponibilitaCopiaPerHub")
        .RequireAuthorization();

        app.MapPost("/api/copie/{copiaId:guid}/esclusioni/{hubId:guid}", async (Guid copiaId, Guid hubId, ClaimsPrincipal user, IHubService hubService, CancellationToken cancellationToken) =>
        {
            await hubService.EscludiCopiaAsync(copiaId, hubId, user.GetUserId(), cancellationToken);
            return Results.NoContent();
        })
        .WithTags("Hub")
        .WithName("EscludiCopiaDaHub")
        .RequireAuthorization();

        app.MapDelete("/api/copie/{copiaId:guid}/esclusioni/{hubId:guid}", async (Guid copiaId, Guid hubId, ClaimsPrincipal user, IHubService hubService, CancellationToken cancellationToken) =>
        {
            await hubService.RimuoviEsclusioneAsync(copiaId, hubId, user.GetUserId(), cancellationToken);
            return Results.NoContent();
        })
        .WithTags("Hub")
        .WithName("RimuoviEsclusioneCopiaHub")
        .RequireAuthorization();

        return app;
    }
}
