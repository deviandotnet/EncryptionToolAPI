using EncryptionToolAPI.DAL.Entities;
using System.Threading.Tasks;

namespace EncryptionToolAPI.BLL.Interfaces
{
    public interface IKeyManagementService
    {
        Task<(ClientApplication Client, string RawApiKey)> CreateClientAsync(string name);
        Task<(string Dek, Guid ClientId)?> GetClientDataKeyAsync(string apiKey);
        Task<bool> RotateClientKeyAsync(string clientId);
    }
}
