using System;

namespace EncryptionToolAPI.DAL.Entities
{
    public class ClientApplication
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string ApiKeyHash { get; set; } = string.Empty;
        public string EncryptedDataKey { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastRotatedAt { get; set; }
    }
}
