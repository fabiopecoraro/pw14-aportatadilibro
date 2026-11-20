using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class HubConfiguration : IEntityTypeConfiguration<Hub>
{
    public void Configure(EntityTypeBuilder<Hub> builder)
    {
        builder.ToTable("Hub");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Nome).HasMaxLength(100).IsRequired();
        builder.Property(h => h.Indirizzo).HasMaxLength(250).IsRequired();

        builder.Property(h => h.Lat).IsRequired();
        builder.Property(h => h.Lng).IsRequired();

        // Indice composito: la ricerca filtra gli Hub per bounding box
        // (Lat/Lng compresi tra i confini della mappa visibile sul frontend).
        builder.HasIndex(h => new { h.Lat, h.Lng });
    }
}
