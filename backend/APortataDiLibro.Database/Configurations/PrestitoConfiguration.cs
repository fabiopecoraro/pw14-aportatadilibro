using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class PrestitoConfiguration : IEntityTypeConfiguration<Prestito>
{
    public void Configure(EntityTypeBuilder<Prestito> builder)
    {
        builder.ToTable("Prestiti");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Stato)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.GiorniStimati).IsRequired();

        builder.HasOne(p => p.Richiedente)
            .WithMany()
            .HasForeignKey(p => p.RichiedenteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
