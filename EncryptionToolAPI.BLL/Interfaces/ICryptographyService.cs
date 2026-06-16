namespace EncryptionToolAPI.BLL.Interfaces
{
    /// <summary>
    /// Provides cryptographic operations including encryption, decryption, and key generation.
    /// </summary>
    public interface ICryptographyService
    {
        /// <summary>
        /// Encrypts the provided plaintext using the specified base64-encoded key.
        /// </summary>
        string Encrypt(string plaintext, string base64Key);

        /// <summary>
        /// Decrypts the provided ciphertext using the specified base64-encoded key.
        /// </summary>
        string Decrypt(string ciphertext, string base64Key);

        /// <summary>
        /// Generates a cryptographically secure 256-bit key encoded as a base64 string.
        /// </summary>
        string GenerateKey();
    }
}
