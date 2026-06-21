using EncryptionToolAPI.Api.DTOs;
using EncryptionToolAPI.BLL.Interfaces;
using EncryptionToolAPI.DAL;
using EncryptionToolAPI.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EncryptionToolAPI.Api.Controllers
{
    /// <summary>
    /// Controller for handling cryptographic operations (encryption and decryption).
    /// Exposes both single-item and bulk endpoints.
    /// All endpoints are protected by the <see cref="Middleware.ApiKeyMiddleware"/>, which validates
    /// the <c>X-Api-Key</c> header and injects the client's decrypted DEK into the request context.
    /// </summary>
    [ApiController]
    [Route("api/v1/crypto")]
    public class EncryptionController : ControllerBase
    {
        private readonly ICryptographyService _cryptographyService;
        private readonly EncryptionDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionController"/> class.
        /// </summary>
        public EncryptionController(ICryptographyService cryptographyService, EncryptionDbContext dbContext)
        {
            _cryptographyService = cryptographyService;
            _dbContext = dbContext;
        }

        // ─── Single-Item Endpoints ───────────────────────────────────────────────

        /// <summary>
        /// Encrypts the provided plaintext using the client's Data Encryption Key (DEK).
        /// </summary>
        [HttpPost("encrypt")]
        public async Task<IActionResult> Encrypt([FromBody] EncryptRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var dek = HttpContext.Items["ClientDek"] as string;
            var clientIdStr = HttpContext.Items["ClientId"]?.ToString();

            if (string.IsNullOrEmpty(dek) || !Guid.TryParse(clientIdStr, out var clientId))
                return Unauthorized();

            try
            {
                var ciphertext = _cryptographyService.Encrypt(request.Plaintext, dek);

                _dbContext.AuditLogs.Add(new AuditLog
                {
                    ClientApplicationId = clientId,
                    Operation = "Encrypt"
                });
                await _dbContext.SaveChangesAsync();

                return Ok(new EncryptResponse { Ciphertext = ciphertext });
            }
            catch (Exception ex)
            {
                return BadRequest($"Encryption failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Decrypts the provided ciphertext using the client's Data Encryption Key (DEK).
        /// </summary>
        [HttpPost("decrypt")]
        public async Task<IActionResult> Decrypt([FromBody] DecryptRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var dek = HttpContext.Items["ClientDek"] as string;
            var clientIdStr = HttpContext.Items["ClientId"]?.ToString();

            if (string.IsNullOrEmpty(dek) || !Guid.TryParse(clientIdStr, out var clientId))
                return Unauthorized();

            try
            {
                var plaintext = _cryptographyService.Decrypt(request.Ciphertext, dek);

                _dbContext.AuditLogs.Add(new AuditLog
                {
                    ClientApplicationId = clientId,
                    Operation = "Decrypt"
                });
                await _dbContext.SaveChangesAsync();

                return Ok(new DecryptResponse { Plaintext = plaintext });
            }
            catch (Exception ex)
            {
                return BadRequest($"Decryption failed: {ex.Message}");
            }
        }

        // ─── Bulk Endpoints ──────────────────────────────────────────────────────

        /// <summary>
        /// Bulk-encrypts a dictionary of plaintext values using the client's DEK.
        /// The request dictionary maps caller-assigned Row IDs to plaintext strings.
        /// The response dictionary maps those same Row IDs to base64-encoded ciphertext strings,
        /// allowing the client to correlate results to database rows without ambiguity.
        /// </summary>
        /// <remarks>
        /// <para><strong>Security:</strong> The DEK is never transmitted; it is resolved from the
        /// client's API key by the middleware and injected via <c>HttpContext.Items</c>.</para>
        /// <para><strong>Limits:</strong> A maximum of 1,000 items are allowed per request.
        /// Clients with more rows must chunk their calls.</para>
        /// <para><strong>Audit:</strong> A single <c>AuditLog</c> row is inserted recording the
        /// count (e.g., "BulkEncrypt (Count: 100)") rather than one row per item.</para>
        /// </remarks>
        [HttpPost("encrypt/bulk")]
        public async Task<IActionResult> BulkEncrypt([FromBody] BulkEncryptRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var dek = HttpContext.Items["ClientDek"] as string;
            var clientIdStr = HttpContext.Items["ClientId"]?.ToString();

            if (string.IsNullOrEmpty(dek) || !Guid.TryParse(clientIdStr, out var clientId))
                return Unauthorized();

            var results = new Dictionary<string, string>(request.Items.Count);

            try
            {
                // Process every item. If any single encryption fails, the exception
                // propagates and the entire batch is rejected — no partial results are returned.
                foreach (var (rowId, plaintext) in request.Items)
                {
                    results[rowId] = _cryptographyService.Encrypt(plaintext, dek);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Bulk encryption failed: {ex.Message}");
            }

            // Write a single consolidated audit entry for the entire batch.
            _dbContext.AuditLogs.Add(new AuditLog
            {
                ClientApplicationId = clientId,
                Operation = $"BulkEncrypt (Count: {request.Items.Count})"
            });
            await _dbContext.SaveChangesAsync();

            return Ok(new BulkEncryptResponse { Results = results });
        }

        /// <summary>
        /// Bulk-decrypts a dictionary of ciphertext values using the client's DEK.
        /// The request dictionary maps caller-assigned Row IDs to base64-encoded ciphertext strings.
        /// The response dictionary maps those same Row IDs to decrypted plaintext strings,
        /// allowing a frontend datatable to display all rows in a single round-trip.
        /// </summary>
        /// <remarks>
        /// <para><strong>Security:</strong> The DEK is never transmitted; it is resolved from the
        /// client's API key by the middleware and injected via <c>HttpContext.Items</c>.</para>
        /// <para><strong>Atomic failure:</strong> If any single ciphertext fails AES-GCM tag
        /// authentication (e.g., tampered or corrupted data), the entire batch is rejected with
        /// HTTP 400. This prevents an attacker from probing which individual rows are valid
        /// by submitting mixed batches.</para>
        /// <para><strong>Limits:</strong> A maximum of 1,000 items are allowed per request.
        /// Clients with more rows must chunk their calls.</para>
        /// <para><strong>Audit:</strong> A single <c>AuditLog</c> row is inserted recording the
        /// count (e.g., "BulkDecrypt (Count: 100)") rather than one row per item.</para>
        /// </remarks>
        [HttpPost("decrypt/bulk")]
        public async Task<IActionResult> BulkDecrypt([FromBody] BulkDecryptRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var dek = HttpContext.Items["ClientDek"] as string;
            var clientIdStr = HttpContext.Items["ClientId"]?.ToString();

            if (string.IsNullOrEmpty(dek) || !Guid.TryParse(clientIdStr, out var clientId))
                return Unauthorized();

            var results = new Dictionary<string, string>(request.Items.Count);

            try
            {
                // Process every item. AES-GCM will throw CryptographicException if any ciphertext
                // fails authentication. We intentionally do not catch per-item: we let the first
                // failure propagate so the batch is atomically rejected.
                foreach (var (rowId, ciphertext) in request.Items)
                {
                    results[rowId] = _cryptographyService.Decrypt(ciphertext, dek);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Bulk decryption failed: one or more ciphertexts are invalid or corrupted. " +
                                  $"Details: {ex.Message}");
            }

            // Write a single consolidated audit entry for the entire batch.
            _dbContext.AuditLogs.Add(new AuditLog
            {
                ClientApplicationId = clientId,
                Operation = $"BulkDecrypt (Count: {request.Items.Count})"
            });
            await _dbContext.SaveChangesAsync();

            return Ok(new BulkDecryptResponse { Results = results });
        }
    }
}
