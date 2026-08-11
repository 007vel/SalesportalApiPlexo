namespace PlexoRepPortal.Services
{
    /// AES-256-CBC encryption for at-rest fields (e.g. bank details) that must never be stored in plaintext.
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
