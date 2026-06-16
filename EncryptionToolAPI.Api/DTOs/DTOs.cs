namespace EncryptionToolAPI.Api.DTOs
{
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
