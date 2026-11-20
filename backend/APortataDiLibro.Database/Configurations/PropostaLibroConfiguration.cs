using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class PropostaLibroConfiguration : IEntityTypeConfiguration<PropostaLibro>
{
    public void Configure(EntityTypeBuilder<PropostaLibro> builder)
    {
        builder.ToTable("ProposteLibro");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Titolo).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Autore).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Isbn).HasMaxLength(20);

        builder.Property(p => p.Stato)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(p => p.Proponente)
            .WithMany()
            .HasForeignKey(p => p.ProponenteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.LibroCreato)
            .WithMany()
            .HasForeignKey(p => p.LibroCreatoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
