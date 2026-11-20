using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class EsclusioneHubLibroConfiguration : IEntityTypeConfiguration<EsclusioneHubLibro>
{
    public void Configure(EntityTypeBuilder<EsclusioneHubLibro> builder)
    {
        builder.ToTable("EsclusioniHubLibro");

        // Chiave composta: una copia puo' essere esclusa al massimo una volta
        // per ciascun Hub.
        builder.HasKey(e => new { e.CopiaLibroId, e.HubId });

        builder.HasOne(e => e.CopiaLibro)
            .WithMany(c => c.Esclusioni)
            .HasForeignKey(e => e.CopiaLibroId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Hub)
            .WithMany(h => h.Esclusioni)
            .HasForeignKey(e => e.HubId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
