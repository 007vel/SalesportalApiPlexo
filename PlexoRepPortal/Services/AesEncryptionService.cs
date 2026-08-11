using System.Security.Cryptography;

namespace PlexoRepPortal.Services
{
    /// Reads a 256-bit key from configuration ("Encryption:Key", base64) and encrypts/decrypts with
    /// AES-CBC. Each call generates a fresh random IV, stored alongside the ciphertext (IV || ciphertext,
    /// base64-encoded as a whole) so no IV needs to be tracked separately.
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;

        public AesEncryptionService(IConfiguration configuration)
        {
            var base64Key = configuration["Encryption:Key"];
            if (string.IsNullOrWhiteSpace(base64Key))
            {
                throw new InvalidOperationException("Encryption:Key is not configured.");
            }

            _key = Convert.FromBase64String(base64Key);
            if (_key.Length != 32)
            {
                throw new InvalidOperationException("Encryption:Key must decode to 32 bytes (AES-256).");
            }
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var combined = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(combined);
        }

        public string Decrypt(string cipherText)
        {
            var combined = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;

            var iv = new byte[aes.IV.Length];
            var cipherBytes = new byte[combined.Length - iv.Length];
            Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(combined, iv.Length, cipherBytes, 0, cipherBytes.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return System.Text.Encoding.UTF8.GetString(plainBytes);
        }
    }
}
