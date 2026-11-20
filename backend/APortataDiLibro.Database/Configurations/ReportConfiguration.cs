using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Report");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Motivo).HasMaxLength(500).IsRequired();

        builder.Property(r => r.Stato)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(r => r.Prestito)
            .WithMany()
            .HasForeignKey(r => r.PrestitoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Segnalante)
            .WithMany()
            .HasForeignKey(r => r.SegnalanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.UtenteSegnalato)
            .WithMany()
            .HasForeignKey(r => r.UtenteSegnalatoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
