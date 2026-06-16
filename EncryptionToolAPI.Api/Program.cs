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
builder.Services.AddOpenApi();

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
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Add Custom API Key Middleware
app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
