using System;

namespace EncryptionToolAPI.DAL.Entities
{
    /// <summary>
    /// Represents a registered consumer web application.
    /// </summary>
    public class ClientApplication
    {
        /// <summary>
        /// The unique identifier for the client application.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The name of the client application.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The SHA-256 hash of the client's API key.
        /// </summary>
        public string ApiKeyHash { get; set; } = string.Empty;

        /// <summary>
        /// The Data Encryption Key (DEK) used for this client, encrypted with the Master Key (KEK).
        /// </summary>
        public string EncryptedDataKey { get; set; } = string.Empty;

        /// <summary>
        /// The UTC timestamp when the client application was registered.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The UTC timestamp when the client's DEK was last rotated, if ever.
        /// </summary>
        public DateTime? LastRotatedAt { get; set; }
    }
}
