namespace APortataDiLibro.Infrastructure.Notifications;

/// <summary>
/// Invio di notifiche email in background. L'implementazione di produzione
/// potra' sostituire <see cref="SimulatedEmailNotificationSender"/> con un
/// client SMTP reale senza impattare i servizi applicativi che la utilizzano.
/// </summary>
public interface IEmailNotificationSender
{
    Task InviaAsync(string destinatarioEmail, string oggetto, string corpo, CancellationToken cancellationToken = default);
}
