using APortataDiLibro.Application.Features.Prestiti;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;

namespace APortataDiLibro.Tests;

/// <summary>
/// Il segnale "gia' recensito" negli elenchi dei prestiti. E' un dato che
/// dipende da chi legge: lo stesso prestito puo' essere recensito da una
/// parte e non dall'altra, e il punto da non sbagliare e' proprio non
/// attribuire a uno la recensione scritta dall'altro.
/// </summary>
public class PrestitiRecensitiTests
{
    private static readonly Guid ProprietarioId = Guid.NewGuid();
    private static readonly Guid RichiedenteId = Guid.NewGuid();
    private static readonly Guid PrestitoId = Guid.NewGuid();

    private static Utente Utente(Guid id, string nickname) => new()
    {
        Id = id,
        Username = nickname.ToLowerInvariant(),
        Nickname = nickname,
        Nome = "Nome",
        Cognome = "Cognome",
        Email = $"{nickname}@example.com",
        PasswordHash = "x",
        DataRegistrazione = DateTime.UtcNow
    };

    private static ApplicationDbContext DbConPrestito(StatoPrestito stato)
    {
        var db = TestDbContextFactory.Crea();
        var libro = new Libro { Id = Guid.NewGuid(), Titolo = "Il nome della rosa", Autore = "Umberto Eco" };
        var copia = new CopiaLibro { Id = Guid.NewGuid(), LibroId = libro.Id, ProprietarioId = ProprietarioId };

        db.Utenti.AddRange(Utente(ProprietarioId, "Proprietaria"), Utente(RichiedenteId, "Richiedente"));
        db.Libri.Add(libro);
        db.CopieLibro.Add(copia);
        db.Prestiti.Add(new Prestito
        {
            Id = PrestitoId,
            CopiaLibroId = copia.Id,
            RichiedenteId = RichiedenteId,
            Stato = stato,
            DataRichiesta = DateTime.UtcNow.AddDays(-20),
            GiorniStimati = 14
        });

        db.SaveChanges();
        return db;
    }

    // Gli elenchi non notificano nessuno: il servizio delle sottoscrizioni non
    // entra in questo percorso.
    private static PrestitoService Servizio(ApplicationDbContext db) => new(db, null!);

    [Fact]
    public async Task PrestitoConcluso_senzaRecensioni_nonRisultaRecensito()
    {
        using var db = DbConPrestito(StatoPrestito.Completato);

        var inviati = await Servizio(db).GetPrestitiInviatiAsync(RichiedenteId);

        Assert.False(Assert.Single(inviati).GiaRecensito);
    }

    [Fact]
    public async Task LaRecensioneDiUnaParte_nonValePerLAltra()
    {
        using var db = DbConPrestito(StatoPrestito.Completato);
        db.RecensioniUtente.Add(new RecensioneUtente
        {
            Id = Guid.NewGuid(),
            PrestitoId = PrestitoId,
            AutoreId = RichiedenteId,
            UtenteRecensitoId = ProprietarioId,
            Voto = 5,
            DataCreazione = DateTime.UtcNow
        });
        db.SaveChanges();
        var servizio = Servizio(db);

        var inviati = await servizio.GetPrestitiInviatiAsync(RichiedenteId);
        var ricevuti = await servizio.GetPrestitiRicevutiAsync(ProprietarioId);

        Assert.True(Assert.Single(inviati).GiaRecensito);
        Assert.False(Assert.Single(ricevuti).GiaRecensito);
    }

    [Fact]
    public async Task BastaLaRecensioneDellEsperienzaDiScambio()
    {
        using var db = DbConPrestito(StatoPrestito.Completato);
        db.RecensioniCopia.Add(new RecensioneCopia
        {
            Id = Guid.NewGuid(),
            PrestitoId = PrestitoId,
            AutoreId = ProprietarioId,
            Voto = 4,
            DataCreazione = DateTime.UtcNow
        });
        db.SaveChanges();

        var ricevuti = await Servizio(db).GetPrestitiRicevutiAsync(ProprietarioId);

        Assert.True(Assert.Single(ricevuti).GiaRecensito);
    }
}
