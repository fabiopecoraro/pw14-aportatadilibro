using Microsoft.Extensions.Logging;

namespace APortataDiLibro.Infrastructure.Notifications;

/// <summary>
/// Implementazione "simulata" dell'invio email: registra la notifica nei log
/// applicativi invece di contattare un server SMTP reale. Sufficiente per il
/// progetto universitario, dove l'obiettivo e' dimostrare il workflow di
/// notifica in background senza dipendenze esterne.
/// </summary>
public class SimulatedEmailNotificationSender : IEmailNotificationSender
{
    private readonly ILogger<SimulatedEmailNotificationSender> _logger;

    public SimulatedEmailNotificationSender(ILogger<SimulatedEmailNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task InviaAsync(string destinatarioEmail, string oggetto, string corpo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Email simulata] A: {Destinatario} - Oggetto: {Oggetto} - Corpo: {Corpo}", destinatarioEmail, oggetto, corpo);
        return Task.CompletedTask;
    }
}
