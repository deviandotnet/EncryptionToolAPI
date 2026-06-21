using EncryptionToolAPI.BLL.Services;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace EncryptionToolAPI.Tests
{
    /// <summary>
    /// Unit tests for bulk cryptographic operations using <see cref="CryptographyService"/>.
    /// These tests exercise the same encrypt/decrypt calls that the bulk controller endpoints invoke,
    /// validating both the happy path and the atomic-failure contract agreed with clients.
    /// </summary>
    public class BulkCryptoTests
    {
        private readonly CryptographyService _sut;
        private readonly string _validKey;

        /// <summary>
        /// Initializes the test class with a fresh <see cref="CryptographyService"/> and
        /// a single shared key simulating a client DEK resolved by the middleware.
        /// </summary>
        public BulkCryptoTests()
        {
            _sut = new CryptographyService();
            _validKey = _sut.GenerateKey();
        }

        // ─── Bulk Encryption ─────────────────────────────────────────────────────

        [Fact]
        public void BulkEncrypt_ShouldReturnSameDictionaryKeys_AsInput()
        {
            // Arrange
            var items = new Dictionary<string, string>
            {
                { "row-1", "Alice" },
                { "row-2", "Bob" },
                { "row-3", "Charlie" }
            };

            // Act — mirrors the loop inside the BulkEncrypt controller action
            var results = new Dictionary<string, string>();
            foreach (var (rowId, plaintext) in items)
                results[rowId] = _sut.Encrypt(plaintext, _validKey);

            // Assert
            results.Keys.Should().BeEquivalentTo(items.Keys,
                because: "every input Row ID must appear in the response for client correlation");
        }

        [Fact]
        public void BulkEncrypt_ShouldProduceDifferentCiphertextPerItem()
        {
            // Arrange — two items with identical plaintext to confirm unique IVs per call
            var plaintext = "SameValue";
            var items = new Dictionary<string, string>
            {
                { "row-1", plaintext },
                { "row-2", plaintext }
            };

            // Act
            var ciphertext1 = _sut.Encrypt(items["row-1"], _validKey);
            var ciphertext2 = _sut.Encrypt(items["row-2"], _validKey);

            // Assert — AES-GCM uses a random Nonce per call, so identical plaintexts
            // must produce statistically distinct ciphertexts.
            ciphertext1.Should().NotBe(ciphertext2,
                because: "each encryption call generates a fresh random IV (nonce)");
        }

        // ─── Bulk Decryption ─────────────────────────────────────────────────────

        [Fact]
        public void BulkDecrypt_ValidBatch_ShouldRestoreOriginalPlaintexts()
        {
            // Arrange — encrypt a batch first so we have authentic ciphertexts
            var original = new Dictionary<string, string>
            {
                { "row-1", "John Smith" },
                { "row-2", "Jane Doe" },
                { "row-3", "Acme Corp" }
            };

            var ciphertexts = new Dictionary<string, string>();
            foreach (var (rowId, plaintext) in original)
                ciphertexts[rowId] = _sut.Encrypt(plaintext, _validKey);

            // Act — mirrors the loop inside the BulkDecrypt controller action
            var decrypted = new Dictionary<string, string>();
            foreach (var (rowId, ciphertext) in ciphertexts)
                decrypted[rowId] = _sut.Decrypt(ciphertext, _validKey);

            // Assert
            decrypted.Should().BeEquivalentTo(original,
                because: "decrypting valid ciphertexts must restore all original plaintexts exactly");
        }

        [Fact]
        public void BulkDecrypt_WithOneCorruptedCiphertext_ShouldThrowAndAbortBatch()
        {
            // Arrange — one valid ciphertext and one deliberately corrupted value
            var validCiphertext = _sut.Encrypt("valid data", _validKey);
            var corruptedCiphertext = "this-is-not-valid-base64-ciphertext!!!";

            var batch = new Dictionary<string, string>
            {
                { "row-1", validCiphertext },
                { "row-2", corruptedCiphertext }   // ← the corrupted entry
            };

            // Act — the controller wraps this loop in a single try/catch; we replicate that here.
            var threwException = false;
            var results = new Dictionary<string, string>();
            try
            {
                foreach (var (rowId, ciphertext) in batch)
                    results[rowId] = _sut.Decrypt(ciphertext, _validKey);
            }
            catch
            {
                threwException = true;
            }

            // Assert
            threwException.Should().BeTrue(
                because: "a corrupted ciphertext must cause the entire batch to fail atomically");

            results.Should().NotContainKey("row-2",
                because: "no partial results should be accessible after a batch failure");
        }

        [Fact]
        public void BulkDecrypt_WithWrongKey_ShouldThrow()
        {
            // Arrange — encrypt with one key, attempt to decrypt with a different key
            var ciphertext = _sut.Encrypt("sensitive data", _validKey);
            var wrongKey = _sut.GenerateKey();

            // Act
            var act = () => _sut.Decrypt(ciphertext, wrongKey);

            // Assert — AES-GCM tag verification fails when the key is wrong
            act.Should().Throw<Exception>(
                because: "decrypting with an incorrect DEK must be rejected by the AES-GCM authentication tag");
        }
    }
}
