namespace APortataDiLibro.Application.Abstractions;

/// <summary>
/// Astrazione per l'hashing e la verifica delle password utente, definita
/// nell'Application layer in quanto operazione di logica di dominio (non I/O
/// esterno) ma isolata dietro interfaccia per consentire la sostituzione
/// dell'algoritmo senza impattare i servizi che la consumano.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Genera l'hash, comprensivo di salt e parametri, da persistere su <c>Utente.PasswordHash</c>.</summary>
    string Hash(string password);

    /// <summary>Verifica che <paramref name="password"/> corrisponda all'hash fornito.</summary>
    bool Verify(string password, string hash);
}
