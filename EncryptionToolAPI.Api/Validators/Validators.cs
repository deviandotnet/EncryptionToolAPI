using EncryptionToolAPI.Api.DTOs;
using FluentValidation;

namespace EncryptionToolAPI.Api.Validators
{
    public class EncryptRequestValidator : AbstractValidator<EncryptRequest>
    {
        public EncryptRequestValidator()
        {
            RuleFor(x => x.Plaintext).NotEmpty().WithMessage("Plaintext is required for encryption.");
        }
    }

    public class DecryptRequestValidator : AbstractValidator<DecryptRequest>
    {
        public DecryptRequestValidator()
        {
            RuleFor(x => x.Ciphertext).NotEmpty().WithMessage("Ciphertext is required for decryption.");
        }
    }

    public class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
    {
        public CreateClientRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Client Name must be between 1 and 100 characters.");
        }
    }

    public class RotateKeyRequestValidator : AbstractValidator<RotateKeyRequest>
    {
        public RotateKeyRequestValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty().WithMessage("ClientId is required.");
        }
    }
}
