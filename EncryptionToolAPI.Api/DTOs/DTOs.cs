namespace EncryptionToolAPI.Api.DTOs
{
    public class EncryptRequest
    {
        public string Plaintext { get; set; } = string.Empty;
    }

    public class EncryptResponse
    {
        public string Ciphertext { get; set; } = string.Empty;
    }

    public class DecryptRequest
    {
        public string Ciphertext { get; set; } = string.Empty;
    }

    public class DecryptResponse
    {
        public string Plaintext { get; set; } = string.Empty;
    }

    public class CreateClientRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class CreateClientResponse
    {
        public string ClientId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class RotateKeyRequest
    {
        public string ClientId { get; set; } = string.Empty;
    }
}
