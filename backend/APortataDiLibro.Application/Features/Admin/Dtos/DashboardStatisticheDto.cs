namespace APortataDiLibro.Application.Features.Admin.Dtos;

/// <summary>Statistiche aggregate mostrate nella dashboard Admin.</summary>
public record DashboardStatisticheDto(
    int TotaleLibri,
    int TotaleCopie,
    int CopieDisponibili,
    int CopieInPrestito,
    double? DurataMediaPrestitiGiorni,
    int ReportAperti);
