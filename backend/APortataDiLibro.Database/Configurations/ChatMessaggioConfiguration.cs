using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class ChatMessaggioConfiguration : IEntityTypeConfiguration<ChatMessaggio>
{
    public void Configure(EntityTypeBuilder<ChatMessaggio> builder)
    {
        builder.ToTable("MessaggiChat");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Testo).HasMaxLength(2000).IsRequired();

        // Recupero ordinato della cronologia per prestito (vista chat e ispezione Admin).
        builder.HasIndex(m => new { m.PrestitoId, m.DataInvio });

        builder.HasOne(m => m.Prestito)
            .WithMany()
            .HasForeignKey(m => m.PrestitoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Mittente)
            .WithMany()
            .HasForeignKey(m => m.MittenteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
