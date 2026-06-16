using EncryptionToolAPI.BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace EncryptionToolAPI.Api.Middleware
{
    /// <summary>
    /// Middleware that intercepts requests to authenticate clients using the X-Api-Key header
    /// and injects the client's Data Encryption Key (DEK) into the HttpContext.
    /// </summary>
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeaderName = "X-Api-Key";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyMiddleware"/> class.
        /// </summary>
        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Processes a request to validate the API key.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            // Skip admin endpoints and swagger UI
            var path = context.Request.Path;
            if (path.StartsWithSegments("/api/v1/admin") || 
                path.StartsWithSegments("/swagger") || 
                path.StartsWithSegments("/openapi"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key was not provided.");
                return;
            }

            var keyManagementService = context.RequestServices.GetRequiredService<IKeyManagementService>();
            var clientData = await keyManagementService.GetClientDataKeyAsync(extractedApiKey!);
            
            if (clientData == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }

            // Store the DEK and ClientId in context items so the controller can use them
            context.Items["ClientDek"] = clientData.Value.Dek;
            context.Items["ClientId"] = clientData.Value.ClientId;

            await _next(context);
        }
    }
}
