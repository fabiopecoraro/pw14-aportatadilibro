using APortataDiLibro.Application.Features.Admin;
using APortataDiLibro.Application.Features.Metriche;
using APortataDiLibro.Core.Constants;

namespace APortataDiLibro.API.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AdminPolicy);

        group.MapGet("/statistiche", async (IAdminService adminService, CancellationToken cancellationToken) =>
        {
            var statistiche = await adminService.GetStatisticheAsync(cancellationToken);
            return Results.Ok(statistiche);
        })
        .WithName("GetStatisticheAdmin");

        // Il periodo e' un parametro e non una costante perche' il grafico deve
        // poter cambiare finestra senza un nuovo endpoint; il servizio lo
        // limita comunque a un anno.
        group.MapGet("/metriche", async (int? giorni, IMetricheService metricheService, CancellationToken cancellationToken) =>
        {
            var metriche = await metricheService.GetMetricheUsoAsync(giorni ?? 30, cancellationToken);
            return Results.Ok(metriche);
        })
        .WithName("GetMetricheUso");

        group.MapGet("/catalogo/csv", async (IAdminService adminService, CancellationToken cancellationToken) =>
        {
            var csv = await adminService.EsportaCatalogoCsvAsync(cancellationToken);
            return Results.File(csv, "text/csv", "catalogo.csv");
        })
        .WithName("EsportaCatalogoCsv");

        // Lo stesso periodo di default di /metriche: il pulsante della
        // dashboard esporta la finestra che l'Admin sta guardando, quindi i due
        // endpoint devono rispondere allo stesso parametro nello stesso modo.
        group.MapGet("/metriche/csv", async (int? giorni, IAdminService adminService, CancellationToken cancellationToken) =>
        {
            var esportazione = await adminService.EsportaMetricheUsoCsvAsync(giorni ?? 30, cancellationToken);
            return Results.File(esportazione.Contenuto, "text/csv", esportazione.NomeFile);
        })
        .WithName("EsportaMetricheUsoCsv");

        return app;
    }
}
