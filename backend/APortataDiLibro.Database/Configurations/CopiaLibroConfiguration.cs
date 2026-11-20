using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class CopiaLibroConfiguration : IEntityTypeConfiguration<CopiaLibro>
{
    public void Configure(EntityTypeBuilder<CopiaLibro> builder)
    {
        builder.ToTable("CopieLibro");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.PercorsoCopertina).HasMaxLength(260);
        builder.Property(c => c.PercorsoCopertinaThumbnail).HasMaxLength(260);

        builder.Property(c => c.Stato)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasMany(c => c.Prestiti)
            .WithOne(p => p.CopiaLibro)
            .HasForeignKey(p => p.CopiaLibroId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
