using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class RecensioneLibroConfiguration : IEntityTypeConfiguration<RecensioneLibro>
{
    public void Configure(EntityTypeBuilder<RecensioneLibro> builder)
    {
        builder.ToTable("RecensioniLibro", t => t.HasCheckConstraint("CK_RecensioniLibro_Voto", "\"Voto\" BETWEEN 1 AND 5"));

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Commento).HasMaxLength(1000);

        // Velocizza il calcolo della media voti per libro richiesto dal report CSV Admin.
        builder.HasIndex(r => r.LibroId);

        builder.HasOne(r => r.Libro)
            .WithMany()
            .HasForeignKey(r => r.LibroId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Autore)
            .WithMany()
            .HasForeignKey(r => r.AutoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
