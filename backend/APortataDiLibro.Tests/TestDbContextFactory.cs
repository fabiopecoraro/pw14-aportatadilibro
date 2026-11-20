using APortataDiLibro.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Tests;

/// <summary>
/// Crea un <see cref="ApplicationDbContext"/> isolato su provider InMemory.
/// <para>
/// Ogni test riceve un database con nome univoco: senza, i test
/// condividerebbero lo stato e l'ordine di esecuzione — che xUnit non
/// garantisce — deciderebbe l'esito.
/// </para>
/// <para>
/// <b>Cosa questi test non possono coprire.</b> Il provider InMemory non e'
/// relazionale: non applica vincoli di chiave esterna, non traduce
/// <c>EF.Functions.ILike</c> (specifico di Npgsql) e non esegue SQL nativo.
/// Restano quindi fuori dalla verifica automatica tre cose, tutte verificate a
/// mano contro PostgreSQL: i filtri testuali di catalogo e ricerca, l'upsert
/// <c>ON CONFLICT</c> del contatore di visualizzazioni, e l'indice unico
/// case-insensitive sulle categorie. Quello che segue copre la logica di
/// dominio, che e' dove stanno le decisioni del progetto.
/// </para>
/// </summary>
public static class TestDbContextFactory
{
    public static ApplicationDbContext Crea()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            // Il provider InMemory avverte a ogni query che le transazioni non
            // sono supportate: e' atteso, e il rumore renderebbe illeggibile
            // l'output dei test.
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}
