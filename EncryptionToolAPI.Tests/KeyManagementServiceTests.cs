using EncryptionToolAPI.BLL.Services;
using EncryptionToolAPI.DAL;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EncryptionToolAPI.Tests
{
    public class KeyManagementServiceTests
    {
        private readonly EncryptionDbContext _dbContext;
        private readonly CryptographyService _cryptoService;
        private readonly IConfiguration _configuration;
        private readonly KeyManagementService _sut;

        public KeyManagementServiceTests()
        {
            var options = new DbContextOptionsBuilder<EncryptionDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new EncryptionDbContext(options);
            _cryptoService = new CryptographyService();

            var inMemorySettings = new Dictionary<string, string?> {
                {"MasterKey", _cryptoService.GenerateKey()}
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _sut = new KeyManagementService(_dbContext, _cryptoService, _configuration);
        }

        [Fact]
        public async Task CreateClientAsync_ShouldReturnClientAndRawApiKey()
        {
            // Act
            var result = await _sut.CreateClientAsync("TestClient");

            // Assert
            result.Client.Should().NotBeNull();
            result.Client.Name.Should().Be("TestClient");
            result.RawApiKey.Should().NotBeNullOrEmpty();
            result.Client.EncryptedDataKey.Should().NotBeNullOrEmpty();
            result.Client.ApiKeyHash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetClientDataKeyAsync_ShouldReturnDecryptedDek()
        {
            // Arrange
            var createResult = await _sut.CreateClientAsync("TestClient");

            // Act
            var dataKeyResult = await _sut.GetClientDataKeyAsync(createResult.RawApiKey);

            // Assert
            dataKeyResult.Should().NotBeNull();
            dataKeyResult!.Value.ClientId.Should().Be(createResult.Client.Id);
            dataKeyResult.Value.Dek.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RotateClientKeyAsync_ShouldChangeEncryptedDek()
        {
            // Arrange
            var createResult = await _sut.CreateClientAsync("TestClient");
            var originalEncryptedDek = createResult.Client.EncryptedDataKey;

            // Act
            var success = await _sut.RotateClientKeyAsync(createResult.Client.Id.ToString());

            // Assert
            success.Should().BeTrue();
            var updatedClient = await _dbContext.ClientApplications.FindAsync(createResult.Client.Id);
            updatedClient!.EncryptedDataKey.Should().NotBe(originalEncryptedDek);
            updatedClient.LastRotatedAt.Should().NotBeNull();
        }
    }
}
