using APortataDiLibro.Application.Features.Profilo;
using APortataDiLibro.Application.Features.Profilo.Dtos;
using APortataDiLibro.Application.Security;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;

namespace APortataDiLibro.Tests;

/// <summary>
/// Modifica del proprio profilo e cambio password. Il caso piu' importante e'
/// il tipo di eccezione sulla password attuale errata: deve restare una
/// ValidationException, perche' un 401 verrebbe letto dall'interceptor del
/// frontend come sessione scaduta e butterebbe fuori l'utente per un errore
/// di battitura.
/// </summary>
public class ProfiloServiceTests
{
    private static readonly Guid UtenteId = Guid.NewGuid();
    private readonly Pbkdf2PasswordHasher _hasher = new();

    private ApplicationDbContext DbConUtente(string password = "Demo1234!")
    {
        var db = TestDbContextFactory.Crea();

        db.Utenti.Add(new Utente
        {
            Id = UtenteId,
            Username = "giulia.rossi",
            Nickname = "GiuliaLegge",
            Nome = "Giulia",
            Cognome = "Rossi",
            Email = "giulia@example.com",
            PasswordHash = _hasher.Hash(password),
            DataRegistrazione = DateTime.UtcNow,
            DataAccettazioneTermini = DateTime.UtcNow
        });

        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task Aggiorna_salvaIdatiEliminandoGliSpaziEsterni()
    {
        using var db = DbConUtente();
        var servizio = new ProfiloService(db, _hasher);

        var profilo = await servizio.AggiornaProfiloAsync(
            UtenteId,
            new AggiornaProfiloRequest("  GiuliaLettrice  ", " Giulia ", " Rossi ", " nuova@example.com "));

        Assert.Equal("GiuliaLettrice", profilo.Nickname);
        Assert.Equal("nuova@example.com", profilo.Email);
        // Lo username non e' fra i campi modificabili: e' l'identificativo di
        // accesso ed e' incorporato nei token gia' emessi.
        Assert.Equal("giulia.rossi", profilo.Username);
    }

    [Fact]
    public async Task Aggiorna_rifiutaUnCampoObbligatorioVuoto()
    {
        using var db = DbConUtente();
        var servizio = new ProfiloService(db, _hasher);

        await Assert.ThrowsAsync<ValidationException>(() => servizio.AggiornaProfiloAsync(
            UtenteId, new AggiornaProfiloRequest("  ", "Giulia", "Rossi", "g@example.com")));
    }

    [Fact]
    public async Task Aggiorna_rifiutaUnEmailSenzaChiocciola()
    {
        using var db = DbConUtente();
        var servizio = new ProfiloService(db, _hasher);

        await Assert.ThrowsAsync<ValidationException>(() => servizio.AggiornaProfiloAsync(
            UtenteId, new AggiornaProfiloRequest("Giulia", "Giulia", "Rossi", "non-una-email")));
    }

    [Fact]
    public async Task Aggiorna_segnalaUnConflittoSeLEmailEDiUnAltroUtente()
    {
        using var db = DbConUtente();
        db.Utenti.Add(new Utente
        {
            Id = Guid.NewGuid(),
            Username = "marco",
            Nickname = "MarcoB",
            Nome = "Marco",
            Cognome = "Bianchi",
            Email = "marco@example.com",
            PasswordHash = "h"
        });
        await db.SaveChangesAsync();

        var servizio = new ProfiloService(db, _hasher);

        await Assert.ThrowsAsync<ConflictException>(() => servizio.AggiornaProfiloAsync(
            UtenteId, new AggiornaProfiloRequest("Giulia", "Giulia", "Rossi", "marco@example.com")));
    }

    [Fact]
    public async Task Aggiorna_consenteDiRisalvareLaPropriaEmail()
    {
        using var db = DbConUtente();
        var servizio = new ProfiloService(db, _hasher);

        // Il controllo di unicita' deve escludere l'utente stesso, altrimenti
        // cambiare il solo nickname fallirebbe.
        var profilo = await servizio.AggiornaProfiloAsync(
            UtenteId, new AggiornaProfiloRequest("NuovoNick", "Giulia", "Rossi", "giulia@example.com"));

        Assert.Equal("NuovoNick", profilo.Nickname);
    }

    [Fact]
    public async Task CambiaPassword_conPasswordAttualeCorretta_aggiornaLHash()
    {
        using var db = DbConUtente();
        var servizio = new ProfiloService(db, _hasher);

        await servizio.CambiaPasswordAsync(UtenteId, new CambiaPasswordRequest("Demo1234!", "NuovaPass1!"));

        var utente = db.Utenti.Single();
        Assert.True(_hasher.Verify("NuovaPass1!", utente.PasswordHash));
        Assert.False(_hasher.Verify("Demo1234!", utente.PasswordHash));
    }

    [Fact]
    public async Task CambiaPassword_conPasswordAttualeErrata_nonEUn401()
    {
        using var db = DbConUtente();
        var servizio = new ProfiloService(db, _hasher);

        // Deliberatamente ValidationException (400) e non UnauthorizedException
        // (401): il token e' valido, e' il campo a essere sbagliato.
        await Assert.ThrowsAsync<ValidationException>(
            () => servizio.CambiaPasswordAsync(UtenteId, new CambiaPasswordRequest("sbagliata", "NuovaPass1!")));

        // E la password non deve essere cambiata.
        Assert.True(_hasher.Verify("Demo1234!", db.Utenti.Single().PasswordHash));
    }

    [Fact]
    public async Task CambiaPassword_rifiutaUnaNuovaPasswordTroppoCorta()
    {
        using var db = DbConUtente();
        var servizio = new ProfiloService(db, _hasher);

        await Assert.ThrowsAsync<ValidationException>(
            () => servizio.CambiaPasswordAsync(UtenteId, new CambiaPasswordRequest("Demo1234!", "corta")));
    }

    [Fact]
    public async Task CambiaPassword_rifiutaLaStessaPasswordDiPrima()
    {
        using var db = DbConUtente();
        var servizio = new ProfiloService(db, _hasher);

        await Assert.ThrowsAsync<ValidationException>(
            () => servizio.CambiaPasswordAsync(UtenteId, new CambiaPasswordRequest("Demo1234!", "Demo1234!")));
    }
}
