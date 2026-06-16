using EncryptionToolAPI.DAL.Entities;
using System;
using System.Threading.Tasks;

namespace EncryptionToolAPI.BLL.Interfaces
{
    /// <summary>
    /// Manages client applications and their associated encryption keys.
    /// </summary>
    public interface IKeyManagementService
    {
        /// <summary>
        /// Registers a new client application and returns its details along with the raw API key.
        /// </summary>
        Task<(ClientApplication Client, string RawApiKey)> CreateClientAsync(string name);

        /// <summary>
        /// Retrieves the decrypted Data Encryption Key (DEK) and ClientId for a given API key.
        /// </summary>
        Task<(string Dek, Guid ClientId)?> GetClientDataKeyAsync(string apiKey);

        /// <summary>
        /// Rotates the Data Encryption Key (DEK) for a specific client application.
        /// </summary>
        Task<bool> RotateClientKeyAsync(string clientId);
    }
}
