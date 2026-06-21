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
- **`Controllers/EncryptionController.cs`**: Exposes `/encrypt`, `/decrypt`, `/encrypt/bulk`, and `/decrypt/bulk` endpoints. Retrieves the DEK from the middleware context and calls the BLL to perform cryptography.
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

### Step 4: Testing the API via Swagger UI
Once the application is running, you can interact with it using the built-in Swagger interface:

1. Navigate to `https://localhost:<port>/swagger` in your browser.
2. **Authorize as Admin**: Click the **Authorize** button at the top right. Under the **AdminKey** section, enter the admin key (e.g., `default-admin-key-change-me` or what you set) and click Authorize.
3. **Create a Client**: Scroll down to `POST /api/v1/admin/clients`. Click "Try it out", enter a client name, and click Execute. Copy the `ApiKey` returned in the response body.
4. **Authorize as Client**: Click the **Authorize** button again. Under the **ApiKey** section, paste the key you just copied.
5. **Test Cryptography**: You can now successfully use the `POST /api/v1/crypto/encrypt` and `POST /api/v1/crypto/decrypt` endpoints without getting Unauthorized errors.
6. **Test Bulk Cryptography**: Use the `POST /api/v1/crypto/encrypt/bulk` and `POST /api/v1/crypto/decrypt/bulk` endpoints (see Section 6 for payload examples).

### Step 5: Run the Unit Tests
To ensure the cryptographic services are working perfectly on the host machine:
```bash
dotnet test EncryptionToolAPI.Tests
```

---

## 6. Bulk Encryption & Decryption Endpoints

These endpoints solve the **N+1 HTTP request bottleneck** for consumer applications that display encrypted rows in a datatable. Instead of one HTTP call per row, the client sends a single call with a batch payload.

### Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/v1/crypto/encrypt/bulk` | Encrypt up to 1,000 plaintexts in one call |
| POST | `/api/v1/crypto/decrypt/bulk` | Decrypt up to 1,000 ciphertexts in one call |

Both endpoints require the `X-Api-Key` header just like the single-item endpoints.

### Request / Response Shape

The payload uses a `Dictionary<string, string>` where the **key is a caller-assigned Row ID** (e.g., your database primary key as a string) and the **value is the data** (plaintext or ciphertext). The Row ID is treated as an opaque correlation token by the API.

**Bulk Encrypt Request:**
```json
{
  "items": {
    "1": "John Smith",
    "2": "Jane Doe",
    "3": "Acme Corp"
  }
}
```

**Bulk Encrypt Response:**
```json
{
  "results": {
    "1": "<base64-ciphertext-for-row-1>",
    "2": "<base64-ciphertext-for-row-2>",
    "3": "<base64-ciphertext-for-row-3>"
  }
}
```

**Bulk Decrypt Request:** (same structure, values are ciphertexts)
```json
{
  "items": {
    "1": "<base64-ciphertext-for-row-1>",
    "2": "<base64-ciphertext-for-row-2>"
  }
}
```

### Security Properties

- **DEK never leaves the server**: The client's Data Encryption Key is resolved from the API key by the middleware. It is never present in the request or response body.
- **Atomic failure**: If *any single* ciphertext fails AES-GCM authentication (corrupted or tampered data), the **entire batch** is rejected with HTTP 400 and no partial plaintext is returned. This prevents an attacker from using a mixed batch to probe which rows are valid.
- **DoS protection**: Requests exceeding **1,000 items** are rejected by FluentValidation before any cryptographic work is performed. Client applications with more rows must **chunk** their requests into pages of ≤1,000.
- **Key length limits**: Row ID keys are capped at 200 characters by the validator to prevent log/header injection.

### Audit Logging

A single `AuditLog` row is written per bulk call (e.g., `Operation: "BulkDecrypt (Count: 50)"`) instead of one row per item. This eliminates a secondary write-amplification DoS vector and keeps the audit table manageable.

### Client Integration Pattern (Datatable Example)

```csharp
// 1. Fetch encrypted rows from YOUR database
var rows = await dbContext.MyTable.ToListAsync();

// 2. Build the bulk decrypt payload: { RowId -> Ciphertext }
var payload = new { items = rows.ToDictionary(r => r.Id.ToString(), r => r.EncryptedName) };

// 3. One HTTP call to the EncryptionToolAPI
var response = await httpClient.PostAsJsonAsync("/api/v1/crypto/decrypt/bulk", payload);
var result = await response.Content.ReadFromJsonAsync<BulkDecryptResponse>();

// 4. Map decrypted values back to rows using the Row ID as a key
foreach (var row in rows)
    row.DisplayName = result.Results[row.Id.ToString()];

// 5. Bind rows to your datatable — all values are now readable
```
