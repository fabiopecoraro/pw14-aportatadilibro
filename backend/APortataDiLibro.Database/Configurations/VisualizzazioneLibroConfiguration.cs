using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class VisualizzazioneLibroConfiguration : IEntityTypeConfiguration<VisualizzazioneLibro>
{
    public void Configure(EntityTypeBuilder<VisualizzazioneLibro> builder)
    {
        builder.ToTable("VisualizzazioniLibro");

        // La chiave composta e' anche il vincolo su cui si appoggia l'upsert
        // atomico "ON CONFLICT ... DO UPDATE" usato per incrementare il
        // contatore senza perdere visualizzazioni concorrenti.
        builder.HasKey(v => new { v.LibroId, v.Giorno });

        // La serie temporale della dashboard filtra per intervallo di giorni
        // su tutti i libri: senza questo indice sarebbe una scansione piena.
        builder.HasIndex(v => v.Giorno);

        builder.HasOne(v => v.Libro)
            .WithMany()
            .HasForeignKey(v => v.LibroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
