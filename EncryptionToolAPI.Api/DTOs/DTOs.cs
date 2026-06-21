namespace EncryptionToolAPI.Api.DTOs
{
    // ─── Bulk DTOs ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Request object for bulk-encrypting multiple plaintext values in a single HTTP call.
    /// The dictionary key is a caller-assigned Row ID (e.g., a database primary key string)
    /// used solely for correlation; the API treats it as opaque and never interprets it.
    /// The dictionary value is the plaintext to encrypt.
    /// </summary>
    public class BulkEncryptRequest
    {
        /// <summary>
        /// A map of Row ID → plaintext values to encrypt.
        /// Maximum 1,000 entries; each value must be non-empty.
        /// </summary>
        public Dictionary<string, string> Items { get; set; } = new();
    }

    /// <summary>
    /// Response object returned after a successful bulk encryption operation.
    /// The dictionary key mirrors the Row ID sent in the request for easy client-side correlation.
    /// The dictionary value is the resulting base64-encoded ciphertext (Nonce + Tag + CipherBytes).
    /// </summary>
    public class BulkEncryptResponse
    {
        /// <summary>
        /// A map of Row ID → base64-encoded ciphertext values.
        /// </summary>
        public Dictionary<string, string> Results { get; set; } = new();
    }

    /// <summary>
    /// Request object for bulk-decrypting multiple ciphertext values in a single HTTP call.
    /// The dictionary key is a caller-assigned Row ID used solely for response correlation.
    /// The dictionary value is the base64-encoded ciphertext to decrypt.
    /// </summary>
    public class BulkDecryptRequest
    {
        /// <summary>
        /// A map of Row ID → base64-encoded ciphertext values to decrypt.
        /// Maximum 1,000 entries; each value must be non-empty.
        /// If any single entry fails authentication (e.g., corrupted ciphertext),
        /// the entire batch is rejected.
        /// </summary>
        public Dictionary<string, string> Items { get; set; } = new();
    }

    /// <summary>
    /// Response object returned after a successful bulk decryption operation.
    /// The dictionary key mirrors the Row ID sent in the request for easy client-side correlation.
    /// The dictionary value is the decrypted plaintext.
    /// </summary>
    public class BulkDecryptResponse
    {
        /// <summary>
        /// A map of Row ID → decrypted plaintext values.
        /// </summary>
        public Dictionary<string, string> Results { get; set; } = new();
    }

    // ─── Single-Item DTOs ────────────────────────────────────────────────────────

    /// <summary>
    /// Request object for encrypting plaintext data.
    /// </summary>
    public class EncryptRequest
    {
        /// <summary>
        /// The plaintext data to encrypt.
        /// </summary>
        public string Plaintext { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response object containing encrypted ciphertext data.
    /// </summary>
    public class EncryptResponse
    {
        /// <summary>
        /// The base64-encoded ciphertext, including IV and Authentication Tag.
        /// </summary>
        public string Ciphertext { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request object for decrypting ciphertext data.
    /// </summary>
    public class DecryptRequest
    {
        /// <summary>
        /// The base64-encoded ciphertext to decrypt.
        /// </summary>
        public string Ciphertext { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response object containing decrypted plaintext data.
    /// </summary>
    public class DecryptResponse
    {
        /// <summary>
        /// The decrypted plaintext data.
        /// </summary>
        public string Plaintext { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request object for creating a new client application.
    /// </summary>
    public class CreateClientRequest
    {
        /// <summary>
        /// The name of the client application.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response object containing details of a newly created client application.
    /// </summary>
    public class CreateClientResponse
    {
        /// <summary>
        /// The unique identifier for the new client application.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the new client application.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The raw API key for the new client. This will only be shown once.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request object for rotating a client application's DEK.
    /// </summary>
    public class RotateKeyRequest
    {
        /// <summary>
        /// The unique identifier of the client application to rotate keys for.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;
    }
}
