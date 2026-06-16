using EncryptionToolAPI.BLL.Services;
using FluentAssertions;
using System;
using Xunit;

namespace EncryptionToolAPI.Tests
{
    public class CryptographyServiceTests
    {
        private readonly CryptographyService _sut;

        public CryptographyServiceTests()
        {
            _sut = new CryptographyService();
        }

        [Fact]
        public void Encrypt_And_Decrypt_ShouldReturnOriginalPlaintext()
        {
            // Arrange
            var key = _sut.GenerateKey();
            var plaintext = "This is a super secret message.";

            // Act
            var ciphertext = _sut.Encrypt(plaintext, key);
            var decrypted = _sut.Decrypt(ciphertext, key);

            // Assert
            ciphertext.Should().NotBe(plaintext);
            decrypted.Should().Be(plaintext);
        }

        [Fact]
        public void GenerateKey_ShouldReturnValidBase64String()
        {
            // Act
            var key = _sut.GenerateKey();

            // Assert
            key.Should().NotBeNullOrEmpty();
            var bytes = Convert.FromBase64String(key);
            bytes.Should().HaveCount(32); // 256-bit key
        }
    }
}
