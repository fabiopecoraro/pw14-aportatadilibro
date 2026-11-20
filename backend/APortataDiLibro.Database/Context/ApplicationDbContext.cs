using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Database.Context;

/// <summary>
/// DbContext principale dell'applicazione. La configurazione del provider
/// Npgsql viene effettuata in fase di registrazione del servizio nel layer
/// API, cosi' da mantenere questo progetto privo di dipendenze su
/// configurazione/hosting.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Utente> Utenti => Set<Utente>();

    public DbSet<Hub> Hub => Set<Hub>();

    public DbSet<Libro> Libri => Set<Libro>();

    public DbSet<CopiaLibro> CopieLibro => Set<CopiaLibro>();

    public DbSet<EsclusioneHubLibro> EsclusioniHubLibro => Set<EsclusioneHubLibro>();

    public DbSet<Prestito> Prestiti => Set<Prestito>();

    public DbSet<RecensioneUtente> RecensioniUtente => Set<RecensioneUtente>();

    public DbSet<RecensioneLibro> RecensioniLibro => Set<RecensioneLibro>();

    public DbSet<RecensioneCopia> RecensioniCopia => Set<RecensioneCopia>();

    public DbSet<SottoscrizioneDisponibilita> SottoscrizioniDisponibilita => Set<SottoscrizioneDisponibilita>();

    public DbSet<ChatMessaggio> MessaggiChat => Set<ChatMessaggio>();

    public DbSet<Report> Report => Set<Report>();

    public DbSet<PropostaLibro> ProposteLibro => Set<PropostaLibro>();

    public DbSet<VisualizzazioneLibro> VisualizzazioniLibro => Set<VisualizzazioneLibro>();

    public DbSet<Categoria> Categorie => Set<Categoria>();

    public DbSet<LibroCategoria> LibriCategorie => Set<LibroCategoria>();

    public DbSet<PropostaLibroCategoria> ProposteLibroCategorie => Set<PropostaLibroCategoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
