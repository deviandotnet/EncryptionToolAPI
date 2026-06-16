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
    public class KeyManagementService : IKeyManagementService
    {
        private readonly EncryptionDbContext _dbContext;
        private readonly ICryptographyService _cryptographyService;
        private readonly string _masterKey;

        public KeyManagementService(EncryptionDbContext dbContext, ICryptographyService cryptographyService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _cryptographyService = cryptographyService;
            _masterKey = configuration["MasterKey"] ?? throw new ArgumentNullException("MasterKey is missing in configuration.");
        }

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

        private string GenerateApiKey()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        private string HashApiKey(string apiKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(apiKey);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
