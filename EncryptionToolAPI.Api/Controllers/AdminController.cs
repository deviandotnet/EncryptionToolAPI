using EncryptionToolAPI.Api.DTOs;
using EncryptionToolAPI.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace EncryptionToolAPI.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IKeyManagementService _keyManagementService;
        private readonly string _adminKey;

        public AdminController(IKeyManagementService keyManagementService, IConfiguration configuration)
        {
            _keyManagementService = keyManagementService;
            _adminKey = configuration["AdminKey"] ?? "default-admin-key-change-me"; // Note: Provide via config
        }

        private bool IsAuthorized()
        {
            if (!Request.Headers.TryGetValue("X-Admin-Key", out var providedKey)) return false;
            return providedKey == _adminKey;
        }

        [HttpPost("clients")]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
        {
            if (!IsAuthorized()) return Unauthorized("Invalid Admin Key");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _keyManagementService.CreateClientAsync(request.Name);

            return Ok(new CreateClientResponse
            {
                ClientId = result.Client.Id.ToString(),
                Name = result.Client.Name,
                ApiKey = result.RawApiKey
            });
        }

        [HttpPost("keys/rotate")]
        public async Task<IActionResult> RotateKey([FromBody] RotateKeyRequest request)
        {
            if (!IsAuthorized()) return Unauthorized("Invalid Admin Key");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _keyManagementService.RotateClientKeyAsync(request.ClientId);

            if (!success) return NotFound($"Client {request.ClientId} not found.");

            return Ok(new { Message = "Key rotated successfully." });
        }
    }
}
