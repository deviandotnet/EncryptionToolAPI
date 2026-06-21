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

    // ─── Bulk Validators ─────────────────────────────────────────────────────────

    /// <summary>
    /// Validator for <see cref="BulkEncryptRequest"/>.
    /// Enforces a hard limit of 1,000 items per request to protect against CPU-exhaustion
    /// Denial of Service attacks given the cost of AES-GCM per operation.
    /// </summary>
    public class BulkEncryptRequestValidator : AbstractValidator<BulkEncryptRequest>
    {
        private const int MaxItems = 1000;
        private const int MaxKeyLength = 200;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkEncryptRequestValidator"/> class.
        /// </summary>
        public BulkEncryptRequestValidator()
        {
            RuleFor(x => x.Items)
                .NotNull().WithMessage("Items dictionary is required.")
                .NotEmpty().WithMessage("Items dictionary must not be empty.");

            RuleFor(x => x.Items.Count)
                .LessThanOrEqualTo(MaxItems)
                .WithMessage($"Bulk request cannot exceed {MaxItems} items per call.");

            // Validate each entry: key (Row ID) and value (plaintext) must be non-empty.
            RuleForEach(x => x.Items)
                .ChildRules(entry =>
                {
                    entry.RuleFor(e => e.Key)
                        .NotEmpty().WithMessage("A Row ID key must not be empty.")
                        .MaximumLength(MaxKeyLength).WithMessage($"A Row ID key must not exceed {MaxKeyLength} characters.");

                    entry.RuleFor(e => e.Value)
                        .NotEmpty().WithMessage("A plaintext value must not be empty.");
                });
        }
    }

    /// <summary>
    /// Validator for <see cref="BulkDecryptRequest"/>.
    /// Enforces a hard limit of 1,000 items per request to protect against CPU-exhaustion
    /// Denial of Service attacks given the cost of AES-GCM per operation.
    /// </summary>
    public class BulkDecryptRequestValidator : AbstractValidator<BulkDecryptRequest>
    {
        private const int MaxItems = 1000;
        private const int MaxKeyLength = 200;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkDecryptRequestValidator"/> class.
        /// </summary>
        public BulkDecryptRequestValidator()
        {
            RuleFor(x => x.Items)
                .NotNull().WithMessage("Items dictionary is required.")
                .NotEmpty().WithMessage("Items dictionary must not be empty.");

            RuleFor(x => x.Items.Count)
                .LessThanOrEqualTo(MaxItems)
                .WithMessage($"Bulk request cannot exceed {MaxItems} items per call.");

            // Validate each entry: key (Row ID) and value (ciphertext) must be non-empty.
            RuleForEach(x => x.Items)
                .ChildRules(entry =>
                {
                    entry.RuleFor(e => e.Key)
                        .NotEmpty().WithMessage("A Row ID key must not be empty.")
                        .MaximumLength(MaxKeyLength).WithMessage($"A Row ID key must not exceed {MaxKeyLength} characters.");

                    entry.RuleFor(e => e.Value)
                        .NotEmpty().WithMessage("A ciphertext value must not be empty.");
                });
        }
    }
}
