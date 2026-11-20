using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class SottoscrizioneDisponibilitaConfiguration : IEntityTypeConfiguration<SottoscrizioneDisponibilita>
{
    public void Configure(EntityTypeBuilder<SottoscrizioneDisponibilita> builder)
    {
        builder.ToTable("SottoscrizioniDisponibilita");

        builder.HasKey(s => s.Id);

        // Un utente puo' avere una sola sottoscrizione attiva per libro:
        // evita di inviare notifiche duplicate alla stessa persona.
        builder.HasIndex(s => new { s.LibroId, s.UtenteId }).IsUnique();

        builder.HasOne(s => s.Libro)
            .WithMany()
            .HasForeignKey(s => s.LibroId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Utente)
            .WithMany()
            .HasForeignKey(s => s.UtenteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
