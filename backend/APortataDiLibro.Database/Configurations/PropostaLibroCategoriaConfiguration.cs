using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class PropostaLibroCategoriaConfiguration : IEntityTypeConfiguration<PropostaLibroCategoria>
{
    public void Configure(EntityTypeBuilder<PropostaLibroCategoria> builder)
    {
        builder.ToTable("ProposteLibroCategorie");

        builder.HasKey(pc => new { pc.PropostaLibroId, pc.CategoriaId });

        builder.HasOne(pc => pc.PropostaLibro)
            .WithMany(p => p.Categorie)
            .HasForeignKey(pc => pc.PropostaLibroId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.Categoria)
            .WithMany(c => c.Proposte)
            .HasForeignKey(pc => pc.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
