using System.Security.Claims;
using APortataDiLibro.API.Extensions;
using APortataDiLibro.Application.Features.Recensioni;
using APortataDiLibro.Application.Features.Recensioni.Dtos;

namespace APortataDiLibro.API.Endpoints;

public static class RecensioniEndpoints
{
    public static IEndpointRouteBuilder MapRecensioniEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recensioni").WithTags("Recensioni");

        group.MapPost("/utenti", async (CreaRecensioneUtenteRequest request, ClaimsPrincipal user, IRecensioniService recensioniService, CancellationToken cancellationToken) =>
        {
            var recensione = await recensioniService.CreaRecensioneUtenteAsync(user.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/recensioni/utenti/{recensione.Id}", recensione);
        })
        .WithName("CreaRecensioneUtente")
        .RequireAuthorization();

        group.MapPost("/libri", async (CreaRecensioneLibroRequest request, ClaimsPrincipal user, IRecensioniService recensioniService, CancellationToken cancellationToken) =>
        {
            var recensione = await recensioniService.CreaRecensioneLibroAsync(user.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/recensioni/libri/{recensione.Id}", recensione);
        })
        .WithName("CreaRecensioneLibro")
        .RequireAuthorization();

        group.MapPost("/copie", async (CreaRecensioneCopiaRequest request, ClaimsPrincipal user, IRecensioniService recensioniService, CancellationToken cancellationToken) =>
        {
            var recensione = await recensioniService.CreaRecensioneCopiaAsync(user.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/recensioni/copie/{recensione.Id}", recensione);
        })
        .WithName("CreaRecensioneCopia")
        .RequireAuthorization();

        app.MapGet("/api/utenti/{id:guid}/recensioni", async (Guid id, IRecensioniService recensioniService, CancellationToken cancellationToken) =>
        {
            var recensioni = await recensioniService.GetRecensioniUtenteAsync(id, cancellationToken);
            return Results.Ok(recensioni);
        })
        .WithTags("Recensioni")
        .WithName("GetRecensioniUtente")
        .AllowAnonymous();

        app.MapGet("/api/libri/{id:guid}/recensioni", async (Guid id, IRecensioniService recensioniService, CancellationToken cancellationToken) =>
        {
            var recensioni = await recensioniService.GetRecensioniLibroAsync(id, cancellationToken);
            return Results.Ok(recensioni);
        })
        .WithTags("Recensioni")
        .WithName("GetRecensioniLibro")
        .AllowAnonymous();

        app.MapGet("/api/prestiti/{id:guid}/recensioni", async (Guid id, IRecensioniService recensioniService, CancellationToken cancellationToken) =>
        {
            var recensioni = await recensioniService.GetRecensioniCopiaAsync(id, cancellationToken);
            return Results.Ok(recensioni);
        })
        .WithTags("Recensioni")
        .WithName("GetRecensioniCopiaPrestito")
        .RequireAuthorization();

        return app;
    }
}
