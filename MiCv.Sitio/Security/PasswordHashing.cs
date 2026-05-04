using System.Security.Cryptography;
using System.Text;

namespace MiCv.Sitio.Security;

/// <summary>
/// SHA-256 en hexadecimal minúsculas (UTF-8). Guardar el resultado en <c>Usuario.Contrasenna</c> (64 caracteres hex).
/// </summary>
public static class PasswordHashing
{
    /// <summary>Crea el hash SHA-256 de una contraseña en texto plano (misma lógica que login y registro).</summary>
    /// <exception cref="ArgumentException">Si la contraseña es null o vacía.</exception>
    public static string CreateHash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static bool MatchesSha256Hex(string plaintext, string? storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
            return false;

        plaintext = plaintext?.Trim();
        if (string.IsNullOrEmpty(plaintext))
            return false;

        string expected;
        try
        {
            expected = CreateHash(plaintext);
        }
        catch (ArgumentException)
        {
            return false;
        }

        var actual = storedHash.Trim().ToLowerInvariant();
        if (actual.Length != expected.Length)
            return false;

        var a = Encoding.ASCII.GetBytes(expected);
        var b = Encoding.ASCII.GetBytes(actual);
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}
