# Architecture Design

## Overview
The Encryption Tool API is built using **.NET 8** and strictly follows a **3-Layered Architecture** (N-Tier). This pattern was chosen to strike a balance between structural clarity and simplicity. Given the focused nature of an encryption service, adopting a heavier architecture like Domain-Driven Design (DDD) or Clean Architecture would lead to over-engineering.

## The 3-Layered Structure

### 1. Presentation Layer (API)
- **Role:** Handles incoming HTTP requests, validates input, and returns responses.
- **Components:** Controllers / Minimal API Endpoints, Data Transfer Objects (DTOs), and API routing logic.
- **Responsibility:** Strictly interface-related tasks. It delegates all business logic to the layer below.

### 2. Business Logic Layer (BLL)
- **Role:** The core of the application. Handles the actual cryptographic operations.
- **Components:** Services implementing encryption/decryption logic (e.g., utilizing `System.Security.Cryptography.AesGcm`), key retrieval, and business rules.
- **Responsibility:** Ensuring data is encrypted securely and maintaining data integrity protocols.

### 3. Data Access Layer (DAL)
- **Role:** Handles any interaction with persistent storage.
- **Components:** Repositories, Database Contexts (e.g., Entity Framework Core).
- **Responsibility:** *Note: Depending on finalized requirements, this layer may be extremely lightweight or omitted if the API remains entirely stateless (i.e., keys are passed via Key Vaults and no internal database is required).* If audit logging or API key tracking is required, the DAL will manage those database interactions.

## Security & Endpoint Philosophy
- **Stateless Operations:** The core `/encrypt` and `/decrypt` endpoints should operate statelessly regarding the payload, processing inputs and returning outputs rapidly.
- **Key Security:** Exposing encryption keys via standard endpoints poses a critical security risk. The architecture assumes keys will remain internal to the BLL or an external Key Management Service, ensuring consumer apps never hold the master keys.
