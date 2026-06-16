# Developer Guide & Project Manual

> **[❗ AI AGENT DIRECTIVE ❗]**
> **CRITICAL INSTRUCTION FOR ANY AI ASSISTANT WORKING ON THIS REPOSITORY:** 
> You are required to read this `DEVELOPER_GUIDE.md` file whenever you are tasked with making structural changes, adding new features, or modifying the architecture of this project. If you make any modifications to the project architecture, database schema, or core logic, **YOU MUST** update this manual to reflect those changes. Do not leave this documentation out-of-date.

---

## 1. Project Overview
The **Encryption Tool API** is a centralized service built on **.NET 8** designed to be consumed by internal and external web applications. Its core purpose is to handle data encryption and decryption, offloading cryptographic complexity from consumer applications and securely managing encryption keys on-premise without relying on third-party Key Management Services (KMS).

## 2. Architecture & Structure
The project uses a strict **3-Layered Architecture** (N-Tier) to separate concerns:
- **API (Presentation Layer)**: `EncryptionToolAPI.Api`
- **BLL (Business Logic Layer)**: `EncryptionToolAPI.BLL`
- **DAL (Data Access Layer)**: `EncryptionToolAPI.DAL`
- **Tests**: `EncryptionToolAPI.Tests`

### Key Security Model (KEK/DEK)
To fulfill the on-premise requirement securely:
- **DEK (Data Encryption Key)**: Every client application gets its own unique DEK.
- **KEK (Key Encryption Key / Master Key)**: DEKs are encrypted using a single Master Key before being saved to the SQL database. This ensures keys are never stored in plain text.

---

## 3. Implementation Details (File-by-File)

### Data Access Layer (`EncryptionToolAPI.DAL`)
- **`Entities/ClientApplication.cs`**: Represents a registered consumer web app. Stores the hashed API key and the encrypted DEK.
- **`Entities/AuditLog.cs`**: Tracks every encryption and decryption operation linked to a specific client for compliance and monitoring.
- **`EncryptionDbContext.cs`**: The Entity Framework Core database context configuring the tables and relationships (using SQL Server).

### Business Logic Layer (`EncryptionToolAPI.BLL`)
- **`Services/CryptographyService.cs`**: Implements the `ICryptographyService`. Uses `System.Security.Cryptography.AesGcm` to perform highly secure, authenticated symmetric encryption. It handles generating Nonces and appending Authentication Tags to the ciphertext.
- **`Services/KeyManagementService.cs`**: Implements the `IKeyManagementService`. Responsible for generating new clients, returning the client's decrypted DEK (by decrypting it with the Master Key), and rotating keys safely.

### API Layer (`EncryptionToolAPI.Api`)
- **`DTOs/DTOs.cs`**: Contains all Data Transfer Objects (Requests/Responses) for endpoints.
- **`Middleware/ApiKeyMiddleware.cs`**: Intercepts requests to `/api/v1/crypto/*`. It extracts the `X-Api-Key` header, validates it via the BLL, and injects the decrypted DEK into the `HttpContext.Items` so the Controller can use it statelessly.
- **`Controllers/EncryptionController.cs`**: Exposes `/encrypt` and `/decrypt` endpoints. Retrieves the DEK from the middleware context and calls the BLL to perform cryptography.
- **`Controllers/AdminController.cs`**: Exposes `/admin/clients` and `/admin/keys/rotate`. Protected by an `X-Admin-Key` header instead of a client API key.
- **`Program.cs`**: Wires up Dependency Injection for the BLL services, configures EF Core with SQL Server, and adds the custom middleware.

---

## 4. Coding Standards (Fluent Libraries)
To ensure clean code and clear separation of concerns, this project mandates the use of specific Fluent libraries across the architecture:

- **Fluent EF Core API (DAL)**: Never use DataAnnotations (like `[Required]`, `[MaxLength]`) on entity POCOs. All database mapping and constraints must be configured inside `EncryptionDbContext.OnModelCreating`.
- **FluentValidation (API)**: Never use DataAnnotations on DTOs. Incoming requests are validated automatically via `FluentValidation.AspNetCore`. Place new validators in the `EncryptionToolAPI.Api/Validators` directory.
- **FluentAssertions (Tests)**: Never use standard xUnit `Assert` methods. Write tests using the Fluent syntax (e.g., `result.Should().NotBeNull()`).

---

## 5. Setup & Deployment Guide

Follow these steps to set up the project on a new machine or on-premise server.

### Step 1: Configure Secrets
You need to provide a **MasterKey** (a 256-bit Base64 string used as the KEK) and an **AdminKey** (for the admin endpoints).

> **How to generate a MasterKey?**
> A MasterKey for AES-256-GCM must be a cryptographically secure 32-byte array encoded as Base64. You can generate a valid one instantly by running this command in **PowerShell**:
> ```powershell
> [Convert]::ToBase64String(( [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes(32) ))
> ```

In the `EncryptionToolAPI.Api` project, open `appsettings.json` (or use Environment Variables) and add:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=EncryptionDB;User Id=YOUR_USER;Password=YOUR_PASS;TrustServerCertificate=True;"
  },
  "MasterKey": "YOUR_GENERATED_BASE64_MASTER_KEY",
  "AdminKey": "YOUR_SECURE_ADMIN_PASSWORD"
}
```

### Step 2: Database Migrations
Since the DAL uses EF Core Code-First, you must create the database schema:
1. Open a terminal in the root directory.
2. Run: `dotnet ef migrations add InitialCreate --project EncryptionToolAPI.DAL --startup-project EncryptionToolAPI.Api`
3. Run: `dotnet ef database update --project EncryptionToolAPI.DAL --startup-project EncryptionToolAPI.Api`

### Step 3: Run the Application
Start the API by running:
```bash
dotnet run --project EncryptionToolAPI.Api
```

### Step 4: Run the Tests
To ensure the cryptographic services are working perfectly on the host machine:
```bash
dotnet test EncryptionToolAPI.Tests
```
