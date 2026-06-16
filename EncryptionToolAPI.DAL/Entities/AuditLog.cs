using System;

namespace EncryptionToolAPI.DAL.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClientApplicationId { get; set; }
        public string Operation { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public ClientApplication? ClientApplication { get; set; }
    }
}
