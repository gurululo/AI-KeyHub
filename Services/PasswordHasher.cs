using System.Security.Cryptography;

namespace AI_KeyHub.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32;   // 256 bit
    private const int Iterations = 100000;

    public (string Hash, string Salt) HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(storedSalt);
            expectedHash = Convert.FromBase64String(storedHash);
        }
        catch
        {
            return false;
        }

        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
