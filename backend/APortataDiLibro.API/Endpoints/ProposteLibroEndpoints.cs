using System.Security.Claims;
using APortataDiLibro.API.Extensions;
using APortataDiLibro.Application.Features.Proposte;
using APortataDiLibro.Application.Features.Proposte.Dtos;
using APortataDiLibro.Core.Constants;

namespace APortataDiLibro.API.Endpoints;

public static class ProposteLibroEndpoints
{
    public static IEndpointRouteBuilder MapProposteLibroEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/proposte-libro").WithTags("ProposteLibro").RequireAuthorization();

        group.MapPost("/", async (CreaPropostaLibroRequest request, ClaimsPrincipal user, IPropostaLibroService propostaService, CancellationToken cancellationToken) =>
        {
            var proposta = await propostaService.CreaPropostaAsync(user.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/proposte-libro/{proposta.Id}", proposta);
        })
        .WithName("CreaPropostaLibro");

        group.MapGet("/mie", async (ClaimsPrincipal user, IPropostaLibroService propostaService, CancellationToken cancellationToken) =>
        {
            var proposte = await propostaService.GetPropostePersonaliAsync(user.GetUserId(), cancellationToken);
            return Results.Ok(proposte);
        })
        .WithName("GetMiePropostLibro");

        group.MapGet("/", async (IPropostaLibroService propostaService, CancellationToken cancellationToken) =>
        {
            var proposte = await propostaService.GetPropostInAttesaAsync(cancellationToken);
            return Results.Ok(proposte);
        })
        .WithName("GetPropostInAttesa")
        .RequireAuthorization(AuthConstants.AdminPolicy);

        group.MapPost("/{id:guid}/approva", async (Guid id, IPropostaLibroService propostaService, CancellationToken cancellationToken) =>
        {
            var proposta = await propostaService.ApprovaPropostaAsync(id, cancellationToken);
            return Results.Ok(proposta);
        })
        .WithName("ApprovaPropostaLibro")
        .RequireAuthorization(AuthConstants.AdminPolicy);

        group.MapPost("/{id:guid}/rifiuta", async (Guid id, IPropostaLibroService propostaService, CancellationToken cancellationToken) =>
        {
            var proposta = await propostaService.RifiutaPropostaAsync(id, cancellationToken);
            return Results.Ok(proposta);
        })
        .WithName("RifiutaPropostaLibro")
        .RequireAuthorization(AuthConstants.AdminPolicy);

        return app;
    }
}
