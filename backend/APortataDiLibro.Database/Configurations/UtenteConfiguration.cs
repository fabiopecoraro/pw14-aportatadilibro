using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APortataDiLibro.Database.Configurations;

public class UtenteConfiguration : IEntityTypeConfiguration<Utente>
{
    public void Configure(EntityTypeBuilder<Utente> builder)
    {
        builder.ToTable("Utenti");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username).HasMaxLength(50).IsRequired();
        builder.Property(u => u.Nickname).HasMaxLength(50).IsRequired();
        builder.Property(u => u.Nome).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Cognome).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();

        // Username e Email sono identificativi univoci di accesso, mentre il
        // Nickname puo' ripetersi tra utenti diversi (e' solo etichetta pubblica).
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasMany(u => u.Hub)
            .WithOne(h => h.Utente)
            .HasForeignKey(h => h.UtenteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Copie)
            .WithOne(c => c.Proprietario)
            .HasForeignKey(c => c.ProprietarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
