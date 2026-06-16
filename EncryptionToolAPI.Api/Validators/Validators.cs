using EncryptionToolAPI.Api.DTOs;
using FluentValidation;

namespace EncryptionToolAPI.Api.Validators
{
    /// <summary>
    /// Validator for <see cref="EncryptRequest"/>.
    /// </summary>
    public class EncryptRequestValidator : AbstractValidator<EncryptRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptRequestValidator"/> class.
        /// </summary>
        public EncryptRequestValidator()
        {
            RuleFor(x => x.Plaintext).NotEmpty().WithMessage("Plaintext is required for encryption.");
        }
    }

    /// <summary>
    /// Validator for <see cref="DecryptRequest"/>.
    /// </summary>
    public class DecryptRequestValidator : AbstractValidator<DecryptRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptRequestValidator"/> class.
        /// </summary>
        public DecryptRequestValidator()
        {
            RuleFor(x => x.Ciphertext).NotEmpty().WithMessage("Ciphertext is required for decryption.");
        }
    }

    /// <summary>
    /// Validator for <see cref="CreateClientRequest"/>.
    /// </summary>
    public class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateClientRequestValidator"/> class.
        /// </summary>
        public CreateClientRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Client Name must be between 1 and 100 characters.");
        }
    }

    /// <summary>
    /// Validator for <see cref="RotateKeyRequest"/>.
    /// </summary>
    public class RotateKeyRequestValidator : AbstractValidator<RotateKeyRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RotateKeyRequestValidator"/> class.
        /// </summary>
        public RotateKeyRequestValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty().WithMessage("ClientId is required.");
        }
    }
}
