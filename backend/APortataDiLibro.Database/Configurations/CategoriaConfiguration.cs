using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorie");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).HasMaxLength(50).IsRequired();

        // Indice unico ordinario sul nome. Il vincolo che conta davvero e'
        // pero' quello case-insensitive su lower("Nome"), che non e'
        // esprimibile con la Fluent API ed e' creato via SQL nella
        // migrazione: senza, "Giallo" e "giallo" convivrebbero nel
        // vocabolario, che e' esattamente il problema che il vocabolario
        // controllato esiste per evitare.
        builder.HasIndex(c => c.Nome).IsUnique();
    }
}
