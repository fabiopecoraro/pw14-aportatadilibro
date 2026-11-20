using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class RecensioneUtenteConfiguration : IEntityTypeConfiguration<RecensioneUtente>
{
    public void Configure(EntityTypeBuilder<RecensioneUtente> builder)
    {
        builder.ToTable("RecensioniUtente", t => t.HasCheckConstraint("CK_RecensioniUtente_Voto", "\"Voto\" BETWEEN 1 AND 5"));

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Commento).HasMaxLength(1000);

        // Un solo giudizio di affidabilita' per autore per ciascun prestito concluso.
        builder.HasIndex(r => new { r.PrestitoId, r.AutoreId }).IsUnique();

        builder.HasOne(r => r.Prestito)
            .WithMany()
            .HasForeignKey(r => r.PrestitoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Autore)
            .WithMany()
            .HasForeignKey(r => r.AutoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.UtenteRecensito)
            .WithMany()
            .HasForeignKey(r => r.UtenteRecensitoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
