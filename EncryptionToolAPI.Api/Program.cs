using EncryptionToolAPI.Api.Middleware;
using EncryptionToolAPI.BLL.Interfaces;
using EncryptionToolAPI.BLL.Services;
using EncryptionToolAPI.DAL;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "v1";
    config.Title = "EncryptionToolAPI";
    config.Version = "v1";
    
    config.AddSecurity("ApiKey", System.Linq.Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "X-Api-Key",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Client API Key"
    });
    
    config.AddSecurity("AdminKey", System.Linq.Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "X-Admin-Key",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Admin Key for /api/v1/admin endpoints"
    });
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// DAL: Entity Framework SQL Server
builder.Services.AddDbContext<EncryptionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// BLL Services
builder.Services.AddScoped<ICryptographyService, CryptographyService>();
builder.Services.AddScoped<IKeyManagementService, KeyManagementService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();

// Add Custom API Key Middleware
app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
