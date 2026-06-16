namespace EncryptionToolAPI.BLL.Interfaces
{
    public interface ICryptographyService
    {
        string Encrypt(string plaintext, string base64Key);
        string Decrypt(string ciphertext, string base64Key);
        string GenerateKey();
    }
}
