using System.Security.Cryptography;
using APortataDiLibro.Application.Abstractions;

namespace APortataDiLibro.Application.Security;

/// <summary>
/// Implementazione di <see cref="IPasswordHasher"/> basata su PBKDF2-HMAC-SHA256
/// (<see cref="Rfc2898DeriveBytes"/>), scelta per non introdurre dipendenze
/// aggiuntive oltre a quanto gia' fornito da .NET. Salt e numero di iterazioni
/// sono generati per-password e codificati nello stesso hash, cosi' da poter
/// essere aumentati in futuro senza invalidare le password esistenti.
/// <para>
/// OWASP raccomanda in prima battuta Argon2id, che aggiunge un costo in
/// memoria e non solo in tempo: PBKDF2 resta una scelta accettabile al
/// fattore di lavoro raccomandato, ed e' preferito qui perche' Argon2 su .NET
/// richiede un pacchetto di terze parti. Il passaggio ad Argon2id e' fra gli
/// sviluppi futuri e non invaliderebbe gli hash esistenti, che restano
/// riconoscibili dal proprio prefisso.
/// </para>
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;

    /// <summary>
    /// Fattore di lavoro raccomandato da OWASP (Password Storage Cheat Sheet)
    /// per PBKDF2-HMAC-SHA256.
    /// <para>
    /// Alzarlo e' sicuro per le password esistenti: il numero di iterazioni e'
    /// codificato in ciascun hash e <see cref="Verify"/> lo rilegge da li',
    /// quindi un hash prodotto con un fattore piu' basso continua a validare.
    /// Il valore nuovo si applica da solo alle password create o cambiate da
    /// qui in avanti.
    /// </para>
    /// </summary>
    private const int Iterations = 600_000;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSizeBytes);

        // Formato: {iterazioni}.{salt}.{hash}, tutto in Base64 tranne il contatore.
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
