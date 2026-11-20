using APortataDiLibro.Application.Abstractions;
using APortataDiLibro.Application.Features.Auth.Dtos;
using APortataDiLibro.Core.Constants;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Auth;

/// <summary>
/// Gestisce registrazione e login. La registrazione assegna sempre il ruolo
/// <see cref="APortataDiLibro.Core.Enums.RuoloUtente.Utente"/>: l'attribuzione
/// del ruolo Admin e' un'operazione amministrativa fuori da questo flusso.
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Il consenso e' un presupposto della creazione dell'account, non un
        // dato da registrare dopo averlo creato: senza, non si procede affatto.
        if (!request.AccettazioneTermini)
        {
            throw new ValidationException(
                "Per registrarti devi accettare l'informativa privacy e i termini di utilizzo.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) ||
            request.Password.Length < AuthConstants.LunghezzaMinimaPassword)
        {
            throw new ValidationException(
                $"La password deve contenere almeno {AuthConstants.LunghezzaMinimaPassword} caratteri.");
        }

        var usernameOrEmailInUso = await _dbContext.Utenti
            .AnyAsync(u => u.Username == request.Username || u.Email == request.Email, cancellationToken);

        if (usernameOrEmailInUso)
        {
            throw new ConflictException("Username o email già in uso.");
        }

        var utente = new Utente
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Nickname = request.Nickname,
            Nome = request.Nome,
            Cognome = request.Cognome,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            DataRegistrazione = DateTime.UtcNow,
            DataAccettazioneTermini = DateTime.UtcNow
        };

        _dbContext.Utenti.Add(utente);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(utente);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var utente = await _dbContext.Utenti
            .SingleOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (utente is null || !_passwordHasher.Verify(request.Password, utente.PasswordHash))
        {
            throw new UnauthorizedException("Username o password non corretti.");
        }

        return BuildAuthResponse(utente);
    }

    private AuthResponse BuildAuthResponse(Utente utente) =>
        new(utente.Id, _jwtTokenGenerator.GenerateToken(utente), utente.Username, utente.Nickname, utente.Ruolo);
}
