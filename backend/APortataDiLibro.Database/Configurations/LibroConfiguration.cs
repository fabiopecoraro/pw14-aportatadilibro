using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class LibroConfiguration : IEntityTypeConfiguration<Libro>
{
    public void Configure(EntityTypeBuilder<Libro> builder)
    {
        builder.ToTable("Libri");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Titolo).HasMaxLength(250).IsRequired();
        builder.Property(l => l.Autore).HasMaxLength(150).IsRequired();
        builder.Property(l => l.Isbn).HasMaxLength(13);

        builder.HasMany(l => l.Copie)
            .WithOne(c => c.Libro)
            .HasForeignKey(c => c.LibroId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
