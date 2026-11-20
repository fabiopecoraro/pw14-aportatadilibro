using APortataDiLibro.Application.Security;

namespace APortataDiLibro.Tests;

/// <summary>
/// L'hashing delle password e' l'unico punto del progetto in cui un errore
/// silenzioso avrebbe conseguenze di sicurezza reali, e il formato dell'hash
/// (<c>{iterazioni}.{salt}.{hash}</c>) e' anche il contratto che gli script di
/// seed devono rispettare per produrre utenti che riescono ad accedere.
/// </summary>
public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void PasswordCorretta_eVerificata()
    {
        var hash = _hasher.Hash("Demo1234!");

        Assert.True(_hasher.Verify("Demo1234!", hash));
    }

    [Fact]
    public void PasswordSbagliata_eRifiutata()
    {
        var hash = _hasher.Hash("Demo1234!");

        Assert.False(_hasher.Verify("demo1234!", hash));
        Assert.False(_hasher.Verify("Demo1234", hash));
        Assert.False(_hasher.Verify(string.Empty, hash));
    }

    [Fact]
    public void StessaPassword_produceHashDiversi_perilSaltCasuale()
    {
        var primo = _hasher.Hash("Demo1234!");
        var secondo = _hasher.Hash("Demo1234!");

        // Due hash identici significherebbero salt costante, e quindi
        // vulnerabilita' alle rainbow table.
        Assert.NotEqual(primo, secondo);
        Assert.True(_hasher.Verify("Demo1234!", primo));
        Assert.True(_hasher.Verify("Demo1234!", secondo));
    }

    [Fact]
    public void Hash_rispettaIlFormatoAttesoDagliScriptDiSeed()
    {
        var parti = _hasher.Hash("Demo1234!").Split('.');

        Assert.Equal(3, parti.Length);

        // Non si fissa il valore esatto, che puo' salire: si verifica che sia
        // un contatore leggibile e che non scenda sotto il fattore di lavoro
        // raccomandato da OWASP. Abbassarlo sarebbe una regressione di
        // sicurezza silenziosa, ed e' quello che questa asserzione intercetta.
        Assert.True(int.TryParse(parti[0], out var iterazioni));
        Assert.True(iterazioni >= 600_000, $"Fattore di lavoro troppo basso: {iterazioni}");
        // Base64 non contiene mai il punto: e' cio' che rende sicura la
        // separazione dei tre campi con Split('.').
        Assert.DoesNotContain('.', parti[1]);
        Assert.DoesNotContain('.', parti[2]);
    }

    [Fact]
    public void HashProdottoConUnFattoreDiLavoroPiuBasso_restaValido()
    {
        // Hash reale di "Demo1234!" a 100.000 iterazioni, il fattore usato
        // prima dell'allineamento alla raccomandazione OWASP. E' lo stesso
        // valore che si trova negli utenti di database/02-seed.sql storici.
        //
        // Deve continuare a validare: le iterazioni sono codificate nell'hash
        // e Verify le rilegge da li'. E' la proprieta' che rende possibile
        // alzare il fattore di lavoro senza invalidare le password esistenti,
        // quindi vale la pena verificarla invece di darla per buona.
        const string hashVecchio =
            "100000.2wPsEB4EM8Bk68KO/QaPpA==.0/YLGhph2smWPzfV3ru6MndeLFjt1ryAJKjdzoOCheo=";

        Assert.True(_hasher.Verify("Demo1234!", hashVecchio));
        Assert.False(_hasher.Verify("password-sbagliata", hashVecchio));
    }

    [Theory]
    [InlineData("")]
    [InlineData("non-e-un-hash")]
    [InlineData("abc.def.ghi")]
    [InlineData("600000.solo-due-campi")]
    public void HashMalformato_nonFaEccezione_maRifiuta(string hashMalformato)
    {
        // Un hash corrotto in database non deve far esplodere il login con un
        // 500: deve semplicemente non autenticare.
        var esito = Record.Exception(() => _hasher.Verify("qualcosa", hashMalformato));

        Assert.Null(esito);
        Assert.False(_hasher.Verify("qualcosa", hashMalformato));
    }
}
