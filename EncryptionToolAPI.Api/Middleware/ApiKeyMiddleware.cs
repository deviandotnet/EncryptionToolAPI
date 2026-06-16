using EncryptionToolAPI.BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace EncryptionToolAPI.Api.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeaderName = "X-Api-Key";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip admin endpoints
            if (context.Request.Path.StartsWithSegments("/api/v1/admin"))
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
