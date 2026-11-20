using APortataDiLibro.Application.Abstractions;
using APortataDiLibro.Application.Features.Profilo.Dtos;
using APortataDiLibro.Core.Constants;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Profilo;

/// <summary>
/// Gestisce i dati del profilo dell'utente autenticato.
/// <para>
/// Ogni operazione lavora sull'Id ricavato dal token, mai su un Id passato dal
/// client: non esiste quindi alcun percorso per leggere o modificare il
/// profilo di un altro utente.
/// </para>
/// </summary>
public class ProfiloService : IProfiloService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public ProfiloService(ApplicationDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<ProfiloDto> GetProfiloAsync(Guid utenteId, CancellationToken cancellationToken = default)
    {
        var utente = await _dbContext.Utenti
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == utenteId, cancellationToken)
            ?? throw new NotFoundException(nameof(Utente), utenteId);

        return ToDto(utente);
    }

    public async Task<ProfiloDto> AggiornaProfiloAsync(
        Guid utenteId,
        AggiornaProfiloRequest request,
        CancellationToken cancellationToken = default)
    {
        var nickname = (request.Nickname ?? string.Empty).Trim();
        var nome = (request.Nome ?? string.Empty).Trim();
        var cognome = (request.Cognome ?? string.Empty).Trim();
        var email = (request.Email ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(nickname) ||
            string.IsNullOrWhiteSpace(nome) ||
            string.IsNullOrWhiteSpace(cognome) ||
            string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException("Nickname, nome, cognome ed email sono obbligatori.");
        }

        if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
        {
            throw new ValidationException("L'indirizzo email non è valido.");
        }

        var utente = await _dbContext.Utenti.SingleOrDefaultAsync(u => u.Id == utenteId, cancellationToken)
            ?? throw new NotFoundException(nameof(Utente), utenteId);

        // L'email ha un indice unico: intercettare il conflitto qui produce un
        // 409 leggibile invece di una violazione di vincolo a livello di driver.
        if (!string.Equals(email, utente.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailInUso = await _dbContext.Utenti
                .AnyAsync(u => u.Id != utenteId && u.Email == email, cancellationToken);

            if (emailInUso)
            {
                throw new ConflictException("Questa email è già associata a un altro account.");
            }
        }

        utente.Nickname = nickname;
        utente.Nome = nome;
        utente.Cognome = cognome;
        utente.Email = email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(utente);
    }

    public async Task CambiaPasswordAsync(
        Guid utenteId,
        CambiaPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NuovaPassword) ||
            request.NuovaPassword.Length < AuthConstants.LunghezzaMinimaPassword)
        {
            throw new ValidationException(
                $"La nuova password deve contenere almeno {AuthConstants.LunghezzaMinimaPassword} caratteri.");
        }

        var utente = await _dbContext.Utenti.SingleOrDefaultAsync(u => u.Id == utenteId, cancellationToken)
            ?? throw new NotFoundException(nameof(Utente), utenteId);

        // ValidationException e non UnauthorizedException: il token e' valido,
        // e' il campo compilato a essere sbagliato. Un 401 qui verrebbe
        // interpretato dall'interceptor del frontend come sessione scaduta e
        // butterebbe fuori l'utente per un errore di battitura.
        if (!_passwordHasher.Verify(request.PasswordAttuale ?? string.Empty, utente.PasswordHash))
        {
            throw new ValidationException("La password attuale non è corretta.");
        }

        if (_passwordHasher.Verify(request.NuovaPassword, utente.PasswordHash))
        {
            throw new ValidationException("La nuova password deve essere diversa da quella attuale.");
        }

        utente.PasswordHash = _passwordHasher.Hash(request.NuovaPassword);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ProfiloDto ToDto(Utente utente) =>
        new(utente.Id, utente.Username, utente.Nickname, utente.Nome, utente.Cognome, utente.Email,
            utente.DataRegistrazione, utente.DataAccettazioneTermini);
}
