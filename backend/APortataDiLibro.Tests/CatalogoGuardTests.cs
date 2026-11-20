using APortataDiLibro.Core.Constants;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Core.Validation;

namespace APortataDiLibro.Tests;

/// <summary>
/// Vincolo sull'anno di pubblicazione. E' una guardia in Core proprio perche'
/// tre percorsi di scrittura distinti la condividono: verificarla una volta
/// qui vale per tutti e tre.
/// </summary>
public class CatalogoGuardTests
{
    [Fact]
    public void AnnoNullo_eAccettato_percheIlCampoEFacoltativo()
    {
        var eccezione = Record.Exception(() => CatalogoGuard.EnsureAnnoValido(null));

        Assert.Null(eccezione);
    }

    [Theory]
    [InlineData(1450)] // estremo inferiore incluso
    [InlineData(1980)]
    public void AnnoNellIntervallo_eAccettato(int anno)
    {
        var eccezione = Record.Exception(() => CatalogoGuard.EnsureAnnoValido(anno));

        Assert.Null(eccezione);
    }

    [Fact]
    public void AnnoCorrentePiuUno_eAccettato_perLeEdizioniInUscita()
    {
        var eccezione = Record.Exception(
            () => CatalogoGuard.EnsureAnnoValido(CatalogoConstants.AnnoPubblicazioneMassimo));

        Assert.Null(eccezione);
    }

    [Theory]
    [InlineData(1449)] // appena sotto l'estremo inferiore
    [InlineData(900)]
    [InlineData(0)]
    [InlineData(-100)]
    public void AnnoTroppoAntico_eRifiutato(int anno)
    {
        Assert.Throws<ValidationException>(() => CatalogoGuard.EnsureAnnoValido(anno));
    }

    [Fact]
    public void AnnoOltreIlMargineDiUnAnno_eRifiutato()
    {
        Assert.Throws<ValidationException>(
            () => CatalogoGuard.EnsureAnnoValido(CatalogoConstants.AnnoPubblicazioneMassimo + 1));
    }

    [Theory]
    [InlineData("9788845292613", "9788845292613")]
    [InlineData("978-88-452-9261-3", "9788845292613")] // come stampato in copertina
    [InlineData(" 978 88 452 9261 3 ", "9788845292613")]
    [InlineData("88-452-9261-x", "884529261X")] // ISBN-10 con carattere di controllo
    public void Isbn_vieneRiportatoAlleSoleCifre(string digitato, string atteso)
    {
        Assert.Equal(atteso, CatalogoGuard.NormalizzaIsbn(digitato));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsbnVuoto_diventaNull_percheIlCampoEFacoltativo(string? digitato)
    {
        Assert.Null(CatalogoGuard.NormalizzaIsbn(digitato));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("97888452926131")] // quattordici cifre
    [InlineData("978884529261X")] // la X vale solo in un ISBN-10
    [InlineData("abcdefghij")]
    public void IsbnMalformato_eRifiutato(string digitato)
    {
        Assert.Throws<ValidationException>(() => CatalogoGuard.NormalizzaIsbn(digitato));
    }

    [Theory]
    [InlineData("", "Umberto Eco")]
    [InlineData("Il nome della rosa", "   ")]
    [InlineData(null, null)]
    public void TitoloOAutoreMancanti_sonoRifiutati(string? titolo, string? autore)
    {
        Assert.Throws<ValidationException>(() => CatalogoGuard.EnsureTitoloEAutoreValidi(titolo, autore));
    }

    // Il limite e' quello che l'opera puo' contenere: una proposta che lo
    // superasse non sarebbe piu' approvabile.
    [Fact]
    public void TitoloEAutore_oltreLaLunghezzaMassima_sonoRifiutati()
    {
        var titoloLungo = new string('a', CatalogoConstants.LunghezzaMassimaTitolo + 1);
        var autoreLungo = new string('a', CatalogoConstants.LunghezzaMassimaAutore + 1);

        Assert.Throws<ValidationException>(() => CatalogoGuard.EnsureTitoloEAutoreValidi(titoloLungo, "Autore"));
        Assert.Throws<ValidationException>(() => CatalogoGuard.EnsureTitoloEAutoreValidi("Titolo", autoreLungo));
    }
}
