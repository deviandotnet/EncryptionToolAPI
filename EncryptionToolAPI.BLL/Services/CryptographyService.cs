using EncryptionToolAPI.BLL.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;

namespace EncryptionToolAPI.BLL.Services
{
    public class CryptographyService : ICryptographyService
    {
        // 12 bytes is the standard IV size for AES-GCM
        private const int NonceSize = 12;
        // 16 bytes tag size
        private const int TagSize = 16;

        public string GenerateKey()
        {
            var key = new byte[32]; // 256 bits
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }

        public string Encrypt(string plaintext, string base64Key)
        {
            var key = Convert.FromBase64String(base64Key);
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            var nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            var tag = new byte[TagSize];
            var ciphertextBytes = new byte[plaintextBytes.Length];

            using (var aesGcm = new AesGcm(key, TagSize))
            {
                aesGcm.Encrypt(nonce, plaintextBytes, ciphertextBytes, tag);
            }

            // Combine Nonce + Tag + Ciphertext into one byte array
            var combined = new byte[NonceSize + TagSize + ciphertextBytes.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, combined, NonceSize, TagSize);
            Buffer.BlockCopy(ciphertextBytes, 0, combined, NonceSize + TagSize, ciphertextBytes.Length);

            return Convert.ToBase64String(combined);
        }

        public string Decrypt(string ciphertext, string base64Key)
        {
            var key = Convert.FromBase64String(base64Key);
            var combinedBytes = Convert.FromBase64String(ciphertext);

            if (combinedBytes.Length < NonceSize + TagSize)
                throw new ArgumentException("Invalid ciphertext length.");

            var nonce = new byte[NonceSize];
            var tag = new byte[TagSize];
            var ciphertextBytes = new byte[combinedBytes.Length - NonceSize - TagSize];

            Buffer.BlockCopy(combinedBytes, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(combinedBytes, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(combinedBytes, NonceSize + TagSize, ciphertextBytes, 0, ciphertextBytes.Length);

            var plaintextBytes = new byte[ciphertextBytes.Length];

            using (var aesGcm = new AesGcm(key, TagSize))
            {
                aesGcm.Decrypt(nonce, ciphertextBytes, tag, plaintextBytes);
            }

            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}
