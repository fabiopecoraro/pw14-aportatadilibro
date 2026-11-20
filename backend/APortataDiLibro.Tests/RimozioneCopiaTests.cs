using APortataDiLibro.Application.Features.Catalogo;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;
using APortataDiLibro.Infrastructure.Covers;

namespace APortataDiLibro.Tests;

/// <summary>
/// Rimozione di una copia dalla libreria. Il caso che conta e' l'ordine fra
/// database e disco: la copertina e' un file, e un file cancellato non torna
/// indietro con un rollback. Se la rimozione viene rifiutata, i file devono
/// essere ancora al loro posto.
/// </summary>
public class RimozioneCopiaTests
{
    private static readonly Guid ProprietarioId = Guid.NewGuid();
    private static readonly Guid CopiaId = Guid.NewGuid();

    /// <summary>
    /// Registra le cancellazioni richieste invece di toccare il disco: al test
    /// interessa sapere se e quando il servizio chiede di eliminare i file.
    /// </summary>
    private sealed class CopertineRegistrate : ICoverImageService
    {
        public List<string?> Eliminate { get; } = [];

        public Task<(string PercorsoCopertina, string PercorsoCopertinaThumbnail)> SalvaCopertinaAsync(
            Guid copiaId, Stream immagine, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public void EliminaCopertina(string? percorsoCopertina, string? percorsoCopertinaThumbnail)
            => Eliminate.Add(percorsoCopertina);
    }

    private static ApplicationDbContext DbConCopia()
    {
        var db = TestDbContextFactory.Crea();

        db.CopieLibro.Add(new CopiaLibro
        {
            Id = CopiaId,
            LibroId = Guid.NewGuid(),
            ProprietarioId = ProprietarioId,
            PercorsoCopertina = "/covers/copia.jpg",
            PercorsoCopertinaThumbnail = "/covers/copia_thumb.jpg",
            Stato = StatoCopia.Disponibile
        });

        db.SaveChanges();
        return db;
    }

    // La rimozione usa soltanto il contesto e il servizio delle copertine: gli
    // altri collaboratori del servizio non entrano in questo percorso.
    private static LibroService Servizio(ApplicationDbContext db, ICoverImageService copertine)
        => new(db, null!, null!, null!, copertine);

    [Fact]
    public async Task CopiaSenzaStorico_vieneRimossaInsiemeAllaCopertina()
    {
        using var db = DbConCopia();
        var copertine = new CopertineRegistrate();

        await Servizio(db, copertine).RimuoviCopiaAsync(CopiaId, ProprietarioId);

        Assert.Empty(db.CopieLibro);
        Assert.Equal("/covers/copia.jpg", Assert.Single(copertine.Eliminate));
    }

    [Theory]
    [InlineData(StatoPrestito.Richiesto)]
    [InlineData(StatoPrestito.Rifiutato)]
    [InlineData(StatoPrestito.Completato)]
    public async Task CopiaConStorico_eRifiutata_eLaCopertinaResta(StatoPrestito stato)
    {
        using var db = DbConCopia();
        db.Prestiti.Add(new Prestito
        {
            Id = Guid.NewGuid(),
            CopiaLibroId = CopiaId,
            RichiedenteId = Guid.NewGuid(),
            Stato = stato,
            DataRichiesta = DateTime.UtcNow,
            GiorniStimati = 14
        });
        db.SaveChanges();
        var copertine = new CopertineRegistrate();

        await Assert.ThrowsAsync<ConflictException>(
            () => Servizio(db, copertine).RimuoviCopiaAsync(CopiaId, ProprietarioId));

        Assert.Single(db.CopieLibro);
        Assert.Empty(copertine.Eliminate);
    }

    [Fact]
    public async Task ChiNonEProprietario_nonPuoRimuovere()
    {
        using var db = DbConCopia();
        var copertine = new CopertineRegistrate();

        await Assert.ThrowsAsync<ForbiddenException>(
            () => Servizio(db, copertine).RimuoviCopiaAsync(CopiaId, Guid.NewGuid()));

        Assert.Empty(copertine.Eliminate);
    }
}
