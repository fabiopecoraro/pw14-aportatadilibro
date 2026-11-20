using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class RecensioneCopiaConfiguration : IEntityTypeConfiguration<RecensioneCopia>
{
    public void Configure(EntityTypeBuilder<RecensioneCopia> builder)
    {
        builder.ToTable("RecensioniCopia", t => t.HasCheckConstraint("CK_RecensioniCopia_Voto", "\"Voto\" BETWEEN 1 AND 5"));

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Commento).HasMaxLength(1000);

        // Una sola recensione dell'esperienza di scambio per autore per prestito.
        builder.HasIndex(r => new { r.PrestitoId, r.AutoreId }).IsUnique();

        builder.HasOne(r => r.Prestito)
            .WithMany()
            .HasForeignKey(r => r.PrestitoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Autore)
            .WithMany()
            .HasForeignKey(r => r.AutoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
