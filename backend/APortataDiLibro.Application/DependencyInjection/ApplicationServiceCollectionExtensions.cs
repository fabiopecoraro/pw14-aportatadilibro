using APortataDiLibro.Application.Abstractions;
using APortataDiLibro.Application.Features.Admin;
using APortataDiLibro.Application.Features.Auth;
using APortataDiLibro.Application.Features.Catalogo;
using APortataDiLibro.Application.Features.Categorie;
using APortataDiLibro.Application.Features.Chat;
using APortataDiLibro.Application.Features.Hub;
using APortataDiLibro.Application.Features.Metriche;
using APortataDiLibro.Application.Features.Notifiche;
using APortataDiLibro.Application.Features.Prestiti;
using APortataDiLibro.Application.Features.Profilo;
using APortataDiLibro.Application.Features.Proposte;
using APortataDiLibro.Application.Features.Recensioni;
using APortataDiLibro.Application.Features.Report;
using APortataDiLibro.Application.Features.Ricerca;
using APortataDiLibro.Application.Security;
using APortataDiLibro.Infrastructure.Covers;
using APortataDiLibro.Infrastructure.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace APortataDiLibro.Application.DependencyInjection;

/// <summary>
/// Punto unico di registrazione dei servizi del layer Application, richiamato
/// dal layer API in fase di composizione del container DI.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILibroService, LibroService>();
        services.AddScoped<ICategorieService, CategorieService>();
        services.AddScoped<IHubService, HubService>();
        services.AddScoped<IRicercaService, RicercaService>();
        services.AddScoped<IPrestitoService, PrestitoService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IRecensioniService, RecensioniService>();
        services.AddScoped<IEmailNotificationSender, SimulatedEmailNotificationSender>();
        services.AddScoped<ICoverImageService, MagickCoverImageService>();
        services.AddScoped<ISottoscrizioniService, SottoscrizioniService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAdminBootstrapper, AdminBootstrapper>();
        services.AddScoped<IPropostaLibroService, PropostaLibroService>();
        services.AddScoped<IMetricheService, MetricheService>();
        services.AddScoped<IProfiloService, ProfiloService>();

        return services;
    }
}
