using APortataDiLibro.Application.Abstractions;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Core.Settings;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace APortataDiLibro.Application.Features.Admin;

/// <summary>
/// Crea il primo utente Admin all'avvio, se configurato e se non ne esiste
/// gia' uno.
/// <para>
/// La registrazione pubblica assegna sempre <see cref="RuoloUtente.Utente"/> e
/// non c'e' alcun endpoint di promozione: senza questo passaggio l'unico modo
/// di ottenere un Admin su un database appena migrato sarebbe una UPDATE a
/// mano. Lo script <c>database/02-seed.sql</c> ne crea gia' uno per l'ambiente
/// di test; questo componente copre il caso complementare, cioe' un database
/// creato dalle sole migrazioni EF.
/// </para>
/// </summary>
public class AdminBootstrapper : IAdminBootstrapper
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AdminBootstrapSettings _settings;
    private readonly ILogger<AdminBootstrapper> _logger;

    public AdminBootstrapper(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IOptions<AdminBootstrapSettings> settings,
        ILogger<AdminBootstrapper> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task EnsureAdminAsync(CancellationToken cancellationToken = default)
    {
        if (!_settings.Abilitato)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.Username) ||
            string.IsNullOrWhiteSpace(_settings.Email) ||
            string.IsNullOrWhiteSpace(_settings.Password))
        {
            _logger.LogWarning(
                "AdminBootstrap è abilitato ma incompleto: Username, Email e Password sono obbligatori. Nessun Admin creato.");
            return;
        }

        try
        {
            // Se lo schema non e' ancora allineato, qualunque query fallirebbe
            // con un errore poco leggibile: meglio dirlo esplicitamente e
            // lasciare che l'API si avvii comunque.
            var migrazioniPendenti = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            if (migrazioniPendenti.Any())
            {
                _logger.LogWarning(
                    "Migrazioni non applicate: eseguire database/01-schema.sql (o 'dotnet ef database update') prima del bootstrap dell'Admin.");
                return;
            }

            if (await _dbContext.Utenti.AnyAsync(u => u.Ruolo == RuoloUtente.Admin, cancellationToken))
            {
                return;
            }

            // Username ed Email hanno un indice unico: un utente non-Admin che
            // occupa gia' quei valori va segnalato, non sovrascritto.
            var conflitto = await _dbContext.Utenti.AnyAsync(
                u => u.Username == _settings.Username || u.Email == _settings.Email,
                cancellationToken);

            if (conflitto)
            {
                _logger.LogWarning(
                    "Impossibile creare l'Admin '{Username}': username o email già in uso da un utente non amministratore.",
                    _settings.Username);
                return;
            }

            _dbContext.Utenti.Add(new Utente
            {
                Id = Guid.NewGuid(),
                Username = _settings.Username,
                Nickname = string.IsNullOrWhiteSpace(_settings.Nickname) ? _settings.Username : _settings.Nickname,
                Nome = _settings.Nome,
                Cognome = _settings.Cognome,
                Email = _settings.Email,
                PasswordHash = _passwordHasher.Hash(_settings.Password),
                Ruolo = RuoloUtente.Admin,
                DataRegistrazione = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Creato l'utente Admin '{Username}' dalla configurazione AdminBootstrap. Cambiare la password prima di qualunque uso non locale.",
                _settings.Username);
        }
        catch (Exception ex)
        {
            // Un database non raggiungibile all'avvio e' una condizione
            // ordinaria (container ancora in partenza): non deve impedire
            // all'API di avviarsi.
            _logger.LogWarning(ex, "Bootstrap dell'utente Admin non riuscito.");
        }
    }
}
