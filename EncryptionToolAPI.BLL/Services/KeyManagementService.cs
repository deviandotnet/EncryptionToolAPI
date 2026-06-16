using EncryptionToolAPI.BLL.Interfaces;
using EncryptionToolAPI.DAL;
using EncryptionToolAPI.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EncryptionToolAPI.BLL.Services
{
    /// <summary>
    /// Implements <see cref="IKeyManagementService"/> to manage client applications and KEK/DEK patterns.
    /// </summary>
    public class KeyManagementService : IKeyManagementService
    {
        private readonly EncryptionDbContext _dbContext;
        private readonly ICryptographyService _cryptographyService;
        private readonly string _masterKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyManagementService"/> class.
        /// </summary>
        public KeyManagementService(EncryptionDbContext dbContext, ICryptographyService cryptographyService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _cryptographyService = cryptographyService;
            _masterKey = configuration["MasterKey"] ?? throw new ArgumentNullException("MasterKey is missing in configuration.");
        }

        /// <inheritdoc/>
        public async Task<(ClientApplication Client, string RawApiKey)> CreateClientAsync(string name)
        {
            var rawApiKey = GenerateApiKey();
            var apiKeyHash = HashApiKey(rawApiKey);

            var rawDek = _cryptographyService.GenerateKey();
            var encryptedDek = _cryptographyService.Encrypt(rawDek, _masterKey);

            var client = new ClientApplication
            {
                Name = name,
                ApiKeyHash = apiKeyHash,
                EncryptedDataKey = encryptedDek
            };

            _dbContext.ClientApplications.Add(client);
            await _dbContext.SaveChangesAsync();

            return (client, rawApiKey);
        }

        /// <inheritdoc/>
        public async Task<(string Dek, Guid ClientId)?> GetClientDataKeyAsync(string apiKey)
        {
            var hash = HashApiKey(apiKey);
            var client = await _dbContext.ClientApplications.FirstOrDefaultAsync(c => c.ApiKeyHash == hash);

            if (client == null)
            {
                return null;
            }

            var decryptedDek = _cryptographyService.Decrypt(client.EncryptedDataKey, _masterKey);
            return (decryptedDek, client.Id);
        }

        /// <inheritdoc/>
        public async Task<bool> RotateClientKeyAsync(string clientId)
        {
            if (!Guid.TryParse(clientId, out var id)) return false;

            var client = await _dbContext.ClientApplications.FindAsync(id);
            if (client == null) return false;

            var newRawDek = _cryptographyService.GenerateKey();
            client.EncryptedDataKey = _cryptographyService.Encrypt(newRawDek, _masterKey);
            client.LastRotatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Generates a random 256-bit API key.
        /// </summary>
        private string GenerateApiKey()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Computes a SHA-256 hash of the provided API key.
        /// </summary>
        private string HashApiKey(string apiKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(apiKey);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
