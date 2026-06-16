using EncryptionToolAPI.Api.DTOs;
using EncryptionToolAPI.BLL.Interfaces;
using EncryptionToolAPI.DAL;
using EncryptionToolAPI.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EncryptionToolAPI.Api.Controllers
{
    /// <summary>
    /// Controller for handling cryptographic operations (encryption and decryption).
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

                // Log Audit
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

                // Log Audit
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
    }
}
