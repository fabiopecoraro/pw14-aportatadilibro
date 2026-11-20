using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class LibroCategoriaConfiguration : IEntityTypeConfiguration<LibroCategoria>
{
    public void Configure(EntityTypeBuilder<LibroCategoria> builder)
    {
        builder.ToTable("LibriCategorie");

        builder.HasKey(lc => new { lc.LibroId, lc.CategoriaId });

        builder.HasOne(lc => lc.Libro)
            .WithMany(l => l.Categorie)
            .HasForeignKey(lc => lc.LibroId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict e non Cascade: cancellare una categoria non deve svuotare
        // silenziosamente la classificazione dei libri che la usano. Il
        // servizio intercetta il caso e risponde con un conflitto leggibile.
        builder.HasOne(lc => lc.Categoria)
            .WithMany(c => c.Libri)
            .HasForeignKey(lc => lc.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
