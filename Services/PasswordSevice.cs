using System.Security.Cryptography;
using System.Text;

namespace CarwashApi.Services;

public class PasswordService {
    
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public (string Hash, string Salt) CreatePasswordHash(string password)
    {
       var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
       var passwordBytes = Encoding.UTF8.GetBytes(password);

       var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
        passwordBytes, 
        saltBytes, 
        Iterations, 
        HashAlgorithmName.SHA256,
        HashSize);

       return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    public bool VerifyPassword(string password, string hash, string salt) {
       var saltBytes = Convert.FromBase64String(salt);
       var hashBytesStored = Convert.FromBase64String(hash);
       var passwordBytes = Encoding.UTF8.GetBytes(password);

       var hashBytesComputed = Rfc2898DeriveBytes.Pbkdf2(
        passwordBytes,
        saltBytes, 
        Iterations, 
        HashAlgorithmName.SHA256,
        hashBytesStored.Length);

       return CryptographicOperations.FixedTimeEquals(hashBytesComputed, hashBytesStored);
    }
}