using System.Security.Claims;
using APortataDiLibro.API.Extensions;
using APortataDiLibro.Application.Features.Profilo;
using APortataDiLibro.Application.Features.Profilo.Dtos;

namespace APortataDiLibro.API.Endpoints;

public static class ProfiloEndpoints
{
    public static IEndpointRouteBuilder MapProfiloEndpoints(this IEndpointRouteBuilder app)
    {
        // Nessuna rotta accetta un id utente: l'identita' arriva sempre dal
        // token, quindi non esiste un modo di indirizzare il profilo altrui.
        var group = app.MapGroup("/api/profilo")
            .WithTags("Profilo")
            .RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IProfiloService profiloService, CancellationToken cancellationToken) =>
        {
            var profilo = await profiloService.GetProfiloAsync(user.GetUserId(), cancellationToken);
            return Results.Ok(profilo);
        })
        .WithName("GetProfilo");

        group.MapPut("/", async (
            AggiornaProfiloRequest request,
            ClaimsPrincipal user,
            IProfiloService profiloService,
            CancellationToken cancellationToken) =>
        {
            var profilo = await profiloService.AggiornaProfiloAsync(user.GetUserId(), request, cancellationToken);
            return Results.Ok(profilo);
        })
        .WithName("AggiornaProfilo");

        group.MapPut("/password", async (
            CambiaPasswordRequest request,
            ClaimsPrincipal user,
            IProfiloService profiloService,
            CancellationToken cancellationToken) =>
        {
            await profiloService.CambiaPasswordAsync(user.GetUserId(), request, cancellationToken);
            return Results.NoContent();
        })
        .WithName("CambiaPassword");

        return app;
    }
}
