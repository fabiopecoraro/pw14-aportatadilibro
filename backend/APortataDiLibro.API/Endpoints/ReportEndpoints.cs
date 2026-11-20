using System.Security.Claims;
using APortataDiLibro.API.Extensions;
using APortataDiLibro.Application.Features.Report;
using APortataDiLibro.Application.Features.Report.Dtos;
using APortataDiLibro.Core.Constants;
using APortataDiLibro.Core.Enums;

namespace APortataDiLibro.API.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report").WithTags("Report").RequireAuthorization();

        group.MapPost("/", async (CreaReportRequest request, ClaimsPrincipal user, IReportService reportService, CancellationToken cancellationToken) =>
        {
            var report = await reportService.CreaReportAsync(user.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/report/{report.Id}", report);
        })
        .WithName("CreaReport");

        group.MapGet("/", async (StatoReport? stato, IReportService reportService, CancellationToken cancellationToken) =>
        {
            var report = await reportService.GetReportsAsync(stato, cancellationToken);
            return Results.Ok(report);
        })
        .WithName("GetReports")
        .RequireAuthorization(AuthConstants.AdminPolicy);

        group.MapGet("/{id:guid}", async (Guid id, IReportService reportService, CancellationToken cancellationToken) =>
        {
            var dettaglio = await reportService.GetDettaglioReportAsync(id, cancellationToken);
            return Results.Ok(dettaglio);
        })
        .WithName("GetDettaglioReport")
        .RequireAuthorization(AuthConstants.AdminPolicy);

        group.MapPost("/{id:guid}/chiudi", async (Guid id, IReportService reportService, CancellationToken cancellationToken) =>
        {
            var report = await reportService.ChiudiReportAsync(id, cancellationToken);
            return Results.Ok(report);
        })
        .WithName("ChiudiReport")
        .RequireAuthorization(AuthConstants.AdminPolicy);

        return app;
    }
}
