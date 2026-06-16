using System;

namespace EncryptionToolAPI.DAL.Entities
{
    /// <summary>
    /// Represents an audit log entry for a cryptographic operation.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// The unique identifier for the audit log entry.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The unique identifier of the client application that performed the operation.
        /// </summary>
        public Guid ClientApplicationId { get; set; }

        /// <summary>
        /// The type of operation performed (e.g., Encrypt, Decrypt).
        /// </summary>
        public string Operation { get; set; } = string.Empty;

        /// <summary>
        /// The UTC timestamp when the operation occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The navigation property to the associated client application.
        /// </summary>
        public ClientApplication? ClientApplication { get; set; }
    }
}
