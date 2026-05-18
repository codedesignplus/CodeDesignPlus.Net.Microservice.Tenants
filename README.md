# 🏢 Tenants Microservice

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](LICENSE.md)
[![Tests](https://img.shields.io/badge/tests-passing-success)](tests/)
[![Coverage](https://img.shields.io/badge/coverage-80%25-green)]()
[![Docker](https://img.shields.io/badge/docker-ready-2496ED?logo=docker)](Dockerfile)

A production-ready microservice for managing tenants and organizations in multi-tenant SaaS applications built with .NET 9. Implements Clean Architecture, DDD, and CQRS patterns with complete tenant lifecycle management, data isolation, and license integration.

---

## What is this microservice?

The Tenants microservice represents each organization (company, organization, or business unit) within the platform. It solves the fundamental problem of multi-tenancy: ensuring that every organization's data is completely isolated and that each one has a valid, active subscription (license) to use the platform. When a new customer purchases a plan, this microservice automatically provisions their organization, making it the entry point for onboarding. It is used by the system itself during provisioning and by platform administrators who manage organization data. Every other microservice in the ecosystem depends on Tenants to know which organization a request belongs to.

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Key Features](#-key-features)
- [Technology Stack](#️-technology-stack)
- [Prerequisites](#️-prerequisites)
- [Getting Started](#-getting-started)
- [API Endpoints](#-api-endpoints)
- [gRPC Services](#-grpc-services)
- [Tenant Model](#-tenant-model)
- [Configuration](#️-configuration)
- [Use Cases & Scenarios](#-use-cases--scenarios)
- [Architecture](#️-architecture)
- [Testing](#-testing)
- [Best Practices](#-best-practices)
- [Troubleshooting](#-troubleshooting)
- [Multi-Tenancy Strategy](#-multi-tenancy-strategy)
- [Data Isolation](#-data-isolation)
- [Event-Driven Provisioning](#-event-driven-provisioning)
- [Security](#-security)
- [FAQ](#-faq)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Overview

The Tenants microservice is the foundation of multi-tenant SaaS platforms, providing centralized tenant/organization management with data isolation, license control, and geographic information. It's designed for B2B SaaS platforms, residential management systems, and enterprise applications requiring tenant segregation.

- **Tenant Management**: Complete CRUD operations for organizations/tenants
- **License Integration**: Track license assignments and expiration per tenant
- **Location Support**: Full geographic hierarchy (Country → State → City → Locality → Neighborhood)
- **Document Types**: Support for multiple identification document types (NIT, RUT, CC, etc.)
- **Multi-Tenancy**: Built-in data isolation and tenant context
- **Event-Driven Provisioning**: Automatic tenant creation from payment events
- **Contact Information**: Email, phone, domain, and address management
- **Active/Inactive State**: Control tenant access and lifecycle

### 🚀 Quick Start

```bash
# 1. Start infrastructure services
git clone https://github.com/codedesignplus/CodeDesignPlus.Environment.Dev
cd CodeDesignPlus.Environment.Dev/resources
docker-compose up -d

# 2. Configure Vault secrets
cd ../../tools/vault
./config-vault.sh

# 3. Run the microservice
dotnet run --project src/entrypoints/CodeDesignPlus.Net.Microservice.Tenants.Rest

# 4. Access Swagger UI
open http://localhost:5000/ms-tenants/swagger
```

### 📊 High-Level Architecture

```
┌─────────────┐
│   Client    │
│ Application │
└──────┬──────┘
       │ HTTPS + JWT
       │
┌──────▼──────────────────────────────────────────────┐
│         Tenants Microservice (REST + gRPC)          │
│  ┌──────────────┐  ┌─────────────┐  ┌────────────┐ │
│  │ Controllers  │  │  MediatR    │  │  Handlers  │ │
│  │ REST/gRPC    │─▶│   (CQRS)    │─▶│ (Business) │ │
│  └──────────────┘  └─────────────┘  └────┬───────┘ │
│                                           │         │
│  ┌────────────────────────────────────────▼──────┐ │
│  │       TenantAggregate (DDD)                   │ │
│  │  - Create/Update/Delete                       │ │
│  │  - License Management                         │ │
│  │  - Location Tracking                          │ │
│  └────────────────────────────────────────────────┘ │
└───────┬──────────────────┬──────────────────┬───────┘
        │                  │                  │
   ┌────▼────┐      ┌──────▼──────┐    ┌─────▼─────┐
   │ MongoDB │      │   Redis     │    │ RabbitMQ  │
   │(Tenant  │      │  (Cache)    │    │ (Events)  │
   │  Data)  │      │             │    │           │
   └─────────┘      └─────────────┘    └───────────┘
        
        ┌────────────────────────────────────────┐
        │  Event-Driven Provisioning Flow       │
        │                                        │
        │  Payments MS ──┐                      │
        │                ▼                       │
        │          OrderPaidAndReady            │
        │          DomainEvent                  │
        │                ▼                       │
        │       AsyncWorker Consumer            │
        │                ▼                       │
        │       CreateTenantCommand             │
        └────────────────────────────────────────┘
```

## 🚀 Key Features

### Core Capabilities

- ✅ **Full Tenant Lifecycle**: Create, Read, Update, Delete operations
- ✅ **License Management**: Track active licenses with start/end dates and metadata
- ✅ **Document Types**: Support for NIT, RUT, CC, CE, Passport, and custom types
- ✅ **Geographic Hierarchy**: Complete location data (Country, State, City, Locality, Neighborhood)
- ✅ **Contact Management**: Email, phone (E.164 validated), domain, and physical address
- ✅ **Multi-Protocol Access**: REST API and gRPC services
- ✅ **Event-Driven Provisioning**: Automatic tenant creation from payment confirmation events
- ✅ **Status Management**: Active/Inactive state control
- ✅ **Query Support**: Filter, sort, and paginate with Criteria pattern
- ✅ **Problem Details**: RFC 7807 compliant error responses

### Technical Features

- Clean Architecture with DDD and CQRS
- Domain events for tenant state changes (Created, Updated, Deleted, LicenseUpdated, LocationUpdated)
- MongoDB for persistence with tenant collection
- RabbitMQ for event publishing and async worker
- Redis for distributed caching
- OAuth2/OpenID Connect security
- Multi-tenancy support with tenant context
- Swagger/OpenAPI documentation
- gRPC reflection for dynamic discovery
- Docker containerization
- Comprehensive test coverage (Unit, Integration, Load)

## 🛠️ Technology Stack

### Core
- **.NET 9** - Runtime and framework
- **ASP.NET Core** - Web API framework
- **C# 13** - Programming language

### Storage & Data
- **MongoDB** - Tenant persistence and queries
- **Redis** - Distributed caching

### Messaging & Events
- **RabbitMQ** - Event publishing and async worker queues

### Architecture & Patterns
- **MediatR** - CQRS command/query handling
- **FluentValidation** - Input validation
- **Mapster** - Object mapping
- **NodaTime** - Date/time handling (Instant for UTC timestamps)

### Security & Configuration
- **Vault** - Secret management (MongoDB, RabbitMQ credentials)
- **OAuth2/OpenID Connect** - Authentication
- **JWT Bearer** - Token-based security
- **HTTPS** - Encrypted communication

### DevOps & Testing
- **Docker** - Containerization
- **Kubernetes** - Orchestration (Helm charts)
- **xUnit** - Unit/integration testing
- **k6** - Load testing
- **Swagger/OpenAPI** - API documentation
- **gRPC Reflection** - Service discovery

## ⚙️ Prerequisites

### Required
- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker & Docker Compose** - For infrastructure services
- **MongoDB 6.0+** - Document database
- **Redis 7.0+** - Caching layer
- **RabbitMQ 3.12+** - Message broker

### Optional
- **Vault** - Secret management (can use appsettings for local dev)
- **Kubernetes** - For production deployment

## 🚀 Getting Started

The following instructions will help you set up the project on your local machine for development and testing purposes.

1. Clone the repository:
```bash
git clone <repository-url>
cd CodeDesignPlus.Net.Microservice.Tenants
```

2. Run the MongoDB, Redis, and RabbitMQ services using Docker Compose. Clone this repository [CodeDesignPlus.Environment.Dev](https://github.com/codedesignplus/CodeDesignPlus.Environment.Dev) and run the following command:

```bash
cd resources
docker-compose up -d
```

3. Run the script to configure Vault:

```bash
cd tools/vault
./config-vault.sh
```

4. Build the solution:
```bash
dotnet build
```

5. Run the desired entry point:
   
   - For REST API:
      ```bash
      dotnet run --project src/entrypoints/CodeDesignPlus.Net.Microservice.Tenants.Rest
      ```
      Access Swagger UI at `http://localhost:5000/ms-tenants/swagger`

   - For gRPC:
      ```bash
      dotnet run --project src/entrypoints/CodeDesignPlus.Net.Microservice.Tenants.gRpc
      ```
      gRPC endpoint: `http://localhost:5001`

   - For AsyncWorker:
      ```bash
      dotnet run --project src/entrypoints/CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker
      ```
      Listens to RabbitMQ queues for event-driven tenant provisioning

## 📡 API Endpoints

### Tenant Operations

#### Get All Tenants
```http
GET /api/tenant?page=1&pageSize=10&orderBy=name&sortDirection=asc
Authorization: Bearer {token}
X-Tenant: {tenant-id}
```

**Query Parameters**:
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)
- `orderBy` (string): Field to sort by (default: "createdAt")
- `sortDirection` (string): "asc" or "desc" (default: "desc")
- `filters` (string): JSON filter criteria

**Response**: `200 OK` with paginated results
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Acme Corporation",
      "typeDocument": {
        "code": "NIT",
        "name": "Número de Identificación Tributaria"
      },
      "numberDocument": "900123456-7",
      "domain": "https://acme.com",
      "phone": "+573001234567",
      "email": "contact@acme.com",
      "location": {
        "country": {
          "id": "co",
          "name": "Colombia",
          "alpha2": "CO",
          "alpha3": "COL",
          "code": 170,
          "timezone": "America/Bogota",
          "currency": {
            "code": "COP",
            "name": "Colombian Peso",
            "symbol": "$",
            "decimalDigits": 2,
            "numericCode": 170
          }
        },
        "state": {
          "id": "co-dc",
          "name": "Bogotá D.C.",
          "code": "DC"
        },
        "city": {
          "id": "co-dc-bog",
          "name": "Bogotá",
          "timezone": "America/Bogota"
        },
        "locality": {
          "id": "loc-usaquen",
          "name": "Usaquén"
        },
        "neighborhood": {
          "id": "nb-santa-barbara",
          "name": "Santa Bárbara"
        },
        "address": "Calle 100 #10-20",
        "postalCode": "110111"
      },
      "license": {
        "id": "lic-pro-001",
        "name": "Pro Plan",
        "startDate": "2026-01-01T00:00:00Z",
        "endDate": "2026-12-31T23:59:59Z",
        "metadata": {
          "MaxUsers": "100",
          "MaxProjects": "50",
          "StorageGB": "500"
        }
      },
      "isActive": true
    }
  ],
  "totalItems": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

#### Get Tenant by ID
```http
GET /api/tenant/{id}
Authorization: Bearer {token}
X-Tenant: {tenant-id}
```

**Response**: `200 OK` with tenant details (same structure as above)

#### Create Tenant
```http
POST /api/tenant
Content-Type: application/json
Authorization: Bearer {token}
X-Tenant: {tenant-id}

{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Acme Corporation",
  "typeDocument": {
    "code": "NIT",
    "name": "Número de Identificación Tributaria"
  },
  "numberDocument": "900123456-7",
  "domain": "https://acme.com",
  "phone": "+573001234567",
  "email": "contact@acme.com",
  "location": {
    "country": {
      "id": "co",
      "name": "Colombia",
      "alpha2": "CO",
      "alpha3": "COL",
      "code": 170,
      "timezone": "America/Bogota",
      "currency": {
        "code": "COP",
        "name": "Colombian Peso",
        "symbol": "$",
        "decimalDigits": 2,
        "numericCode": 170
      }
    },
    "state": {
      "id": "co-dc",
      "name": "Bogotá D.C.",
      "code": "DC"
    },
    "city": {
      "id": "co-dc-bog",
      "name": "Bogotá"
    },
    "locality": {
      "id": "loc-usaquen",
      "name": "Usaquén"
    },
    "neighborhood": {
      "id": "nb-santa-barbara",
      "name": "Santa Bárbara"
    },
    "address": "Calle 100 #10-20",
    "postalCode": "110111"
  },
  "license": {
    "id": "lic-pro-001",
    "name": "Pro Plan",
    "startDate": "2026-01-01T00:00:00Z",
    "endDate": "2026-12-31T23:59:59Z",
    "metadata": {
      "MaxUsers": "100",
      "MaxProjects": "50",
      "StorageGB": "500"
    }
  },
  "isActive": true
}
```

**Response**: `204 No Content`

**Domain Events Published**:
- `TenantCreatedDomainEvent` - Published to RabbitMQ for downstream consumers

#### Update Tenant
```http
PUT /api/tenant/{id}
Content-Type: application/json
Authorization: Bearer {token}
X-Tenant: {tenant-id}

{
  "name": "Acme Corporation (Updated)",
  "typeDocument": {
    "code": "NIT",
    "name": "Número de Identificación Tributaria"
  },
  "numberDocument": "900123456-7",
  "domain": "https://acme.com",
  "phone": "+573001234567",
  "email": "contact@acme.com",
  "location": { /* ... */ },
  "license": { /* ... */ },
  "isActive": true
}
```

**Response**: `204 No Content`

**Domain Events Published**:
- `TenantUpdatedDomainEvent` - Published to RabbitMQ

#### Delete Tenant
```http
DELETE /api/tenant/{id}
Authorization: Bearer {token}
X-Tenant: {tenant-id}
```

**Response**: `204 No Content`

**Domain Events Published**:
- `TenantDeletedDomainEvent` - Published to RabbitMQ
- Soft delete: Sets `IsDeleted=true` and `IsActive=false`

## 🔌 gRPC Services

### Service Definition

The microservice exposes a gRPC service for high-performance inter-service communication.

**Service**: `Tenant.Tenant`  
**Proto File**: `src/entrypoints/CodeDesignPlus.Net.Microservice.Tenants.gRpc/Protos/tenant.proto`

### gRPC Methods

#### CreateTenant
```protobuf
rpc CreateTenant (CreateTenantRequest) returns (google.protobuf.Empty);
```

**Request**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Acme Corporation",
  "typeDocument": {
    "code": "NIT",
    "name": "Número de Identificación Tributaria"
  },
  "numbreDocument": "900123456-7",
  "domain": "https://acme.com",
  "phone": "+573001234567",
  "email": "contact@acme.com",
  "location": { /* ... */ },
  "license": { /* ... */ },
  "isActive": true
}
```

#### UpdateTenant
```protobuf
rpc UpdateTenant (UpdateTenantRequest) returns (google.protobuf.Empty);
```

#### DeleteTenant
```protobuf
rpc DeleteTenant (DeleteTenantRequest) returns (google.protobuf.Empty);
```

**Request**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000"
}
```

#### GetTenant
```protobuf
rpc GetTenant (GetTenantRequest) returns (GetTenantResponse);
```

**Request**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Acme Corporation",
  "typeDocument": { /* ... */ },
  "numberDocument": "900123456-7",
  "domain": "https://acme.com",
  "phone": "+573001234567",
  "email": "contact@acme.com",
  "location": { /* ... */ },
  "license": { /* ... */ },
  "isActive": true
}
```

#### ExistTenant
```protobuf
rpc ExistTenant (ExistTenantRequest) returns (google.protobuf.BoolValue);
```

**Request**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response**:
```json
{
  "value": true
}
```

### gRPC Client Example (C#)

```csharp
using Grpc.Net.Client;
using CodeDesignPlus.Net.Microservice.Tenants.gRpc;

var channel = GrpcChannel.ForAddress("http://ms-tenants-grpc:5001");
var client = new Tenant.TenantClient(channel);

// Get tenant
var request = new GetTenantRequest { Id = tenantId.ToString() };
var response = await client.GetTenantAsync(request);

Console.WriteLine($"Tenant: {response.Name}");
```

## 🏢 Tenant Model

### TenantAggregate (Domain)

#### What is it and what is it for?

The TenantAggregate represents a single organization (organization or business unit) in the system. It holds the organization's identity information (name, tax document, contact details), its geographic location, and its active license (subscription plan). Every data operation in the platform is scoped to a specific tenant, making this the foundational entity that enables multi-tenancy and data isolation.

**Properties**:
- `Id` (Guid): Unique tenant identifier
- `Name` (string): Organization name (1-128 characters)
- `TypeDocument` (TypeDocument): Document type (NIT, RUT, CC, CE, etc.)
- `NumberDocument` (string): Document number/identification
- `Phone` (string): Contact phone (E.164 format, e.g., `+573001234567`)
- `Email` (string): Contact email
- `Domain` (Uri?): Organization website URL (optional)
- `Location` (Location): Geographic location hierarchy
- `License` (License): Active license details
- `IsActive` (bool): Tenant active status
- `IsDeleted` (bool): Soft delete flag
- `CreatedAt`, `UpdatedAt`, `DeletedAt` (Instant): Timestamps
- `CreatedBy`, `UpdatedBy`, `DeletedBy` (Guid): User audit trail

### TypeDocument (Value Object)

Represents document type for tenant identification.

**Properties**:
- `Code` (string): Document code (max 3 characters)
- `Name` (string): Document name

**Common Types**:
- `NIT` - Número de Identificación Tributaria (Tax ID)
- `RUT` - Registro Único Tributario (Tax Registry)
- `CC` - Cédula de Ciudadanía (Citizen ID)
- `CE` - Cédula de Extranjería (Foreigner ID)
- `PAS` - Passport

### License (Value Object)

Represents the active license for a tenant.

**Properties**:
- `Id` (Guid): License reference ID
- `Name` (string): License name (1-128 characters)
- `StartDate` (Instant): License start date
- `EndDate` (Instant): License expiration date
- `Metadata` (Dictionary<string, string>): License limits/features
  - Example: `{ "MaxUsers": "100", "MaxProjects": "50", "StorageGB": "500" }`

**Validation**:
- StartDate must be before EndDate
- Name must match regex: `^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{1,128}$`

### Location (Value Object from SDK)

Complete geographic hierarchy for tenant location.

**Properties**:
- `Country` (Country): Country information
  - `Id`, `Name`, `Alpha2` (ISO 3166-1), `Alpha3`, `Code`, `Timezone`, `Currency`
- `State` (State): State/province/department
  - `Id`, `Name`, `Code` (ISO 3166-2)
- `City` (City): City/municipality
  - `Id`, `Name`, `Timezone` (optional)
- `Locality` (Locality): Locality/district
  - `Id`, `Name`
- `Neighborhood` (Neighborhood): Neighborhood/zone
  - `Id`, `Name`
- `Address` (string): Street address
- `PostalCode` (string): ZIP/postal code

**Currency** (nested in Country):
- `Id`, `Code` (ISO 4217), `Name`, `Symbol`, `DecimalDigits`, `NumericCode`

## ⚙️ Configuration

### appsettings.json

```json
{
  "Core": {
    "Id": "587a459a-fb35-4abb-a1fd-59c49a715cf3",
    "PathBase": "/ms-tenants",
    "AppName": "ms-tenants",
    "TypeEntryPoint": "rest",
    "Version": "v1",
    "Description": "Microservice to manage tenants",
    "Business": "CodeDesignPlus",
    "Contact": {
      "Name": "CodeDesignPlus",
      "Email": "support@codedesignplus.com"
    }
  },
  "Security": {
    "ValidateIssuer": true,
    "ValidateLifetime": true,
    "RequireHttpsMetadata": true,
    "ValidIssuer": "https://identity-provider.com",
    "ValidAudiences": ["ms-tenants"],
    "ValidateLicense": true,
    "ValidateRbac": true,
    "ServerRbac": "http://ms-rbac:5001"
  },
  "Mongo": {
    "Enable": true,
    "Database": "db-ms-tenants"
  },
  "Redis": {
    "Instances": {
      "Core": {
        "ConnectionString": "localhost:6379"
      }
    }
  },
  "RedisCache": {
    "Enable": true,
    "Expiration": "00:05:00"
  },
  "RabbitMQ": {
    "Enable": true,
    "Host": "localhost",
    "Port": 5672,
    "UserName": "user",
    "Password": "pass"
  },
  "Vault": {
    "Enable": true,
    "Address": "http://localhost:8200",
    "AppName": "ms-tenants",
    "Solution": "security-codedesignplus",
    "Token": "root",
    "Mongo": {
      "Enable": true,
      "TemplateConnectionString": "mongodb://{0}:{1}@localhost:27017"
    },
    "RabbitMQ": {
      "Enable": true
    }
  },
  "GrpcClients": {
    "Notification": "http://ms-notification-grpc:5001"
  }
}
```

### Environment Variables (Docker/Kubernetes)

```bash
# Core
ASPNETCORE_ENVIRONMENT=Staging
CORE__PATHBASE=/ms-tenants
CORE__APPNAME=ms-tenants

# MongoDB
MONGO__ENABLE=true
MONGO__DATABASE=db-ms-tenants

# Redis
REDIS__INSTANCES__CORE__CONNECTIONSTRING=redis-core:6379
REDISCACHE__ENABLE=true
REDISCACHE__EXPIRATION=00:05:00

# RabbitMQ
RABBITMQ__ENABLE=true
RABBITMQ__HOST=rabbitmq
RABBITMQ__PORT=5672

# Vault
VAULT__ENABLE=true
VAULT__ADDRESS=http://vault:8200
VAULT__APPNAME=ms-tenants
VAULT__SOLUTION=security-codedesignplus
VAULT__TOKEN=s.xxxxxxxxxxxxx

# Security
SECURITY__VALIDATEISSUER=true
SECURITY__VALIDISSUER=https://identity.example.com
SECURITY__VALIDLICENSE=true
SECURITY__VALIDATERBAC=true
SECURITY__SERVERRBAC=http://ms-rbac:5001
```

### Vault Secrets Structure

```
secret/security-codedesignplus/ms-tenants/
├── mongo/
│   ├── username: "ms-tenants-user"
│   └── password: "secure-password"
└── rabbitmq/
    ├── username: "ms-tenants-rmq"
    └── password: "secure-password"
```

## 🎭 Use Cases & Scenarios

### 1. SaaS Platform Onboarding

**Scenario**: A new customer signs up for your SaaS platform and completes payment.

**Flow**:
1. User completes payment on Payments microservice
2. Payments MS publishes `OrderPaidAndReadyForProvisioningDomainEvent` to RabbitMQ
3. Tenants AsyncWorker consumes event
4. `CreateTenantHandler` processes event and creates tenant via `CreateTenantCommand`
5. Tenant aggregate created with license details from payment
6. `TenantCreatedDomainEvent` published to RabbitMQ
7. Downstream services (Users, RBAC, etc.) provision resources for new tenant

**Code Example**:
```csharp
// AsyncWorker Consumer
public class CreateTenantHandler : IEventHandler<OrderPaidAndReadyForProvisioningDomainEvent>
{
    public Task HandleAsync(OrderPaidAndReadyForProvisioningDomainEvent data, CancellationToken token)
    {
        var command = new CreateTenantCommand(
            data.TenantDetail.Id,
            data.TenantDetail.Name,
            data.TenantDetail.TypeDocument,
            data.TenantDetail.NumberDocument,
            string.IsNullOrEmpty(data.TenantDetail.Web) ? null : new Uri(data.TenantDetail.Web),
            data.TenantDetail.Phone,
            data.TenantDetail.Email,
            data.TenantDetail.Location,
            License.Create(data.License.Id, data.License.Name, 
                data.License.StartDate, data.License.EndDate, data.License.Metadata),
            data.BuyerId,
            true
        );

        return _mediator.Send(command, token);
    }
}
```

### 2. License Expiration Management

**Scenario**: A tenant's license is about to expire and needs renewal.

**Flow**:
1. Admin calls `GET /api/tenant?filters={"license.endDate":"<2026-06-01"}`
2. Review expiring licenses
3. Update license via `UpdateTenant` with new license data
4. `TenantLicenseUpdatedDomainEvent` published
5. Downstream services (Notification) send renewal reminders

### 3. Geographic Segmentation

**Scenario**: Report on tenant distribution by country and state.

**Query Example**:
```http
GET /api/tenant?filters={"location.country.alpha2":"CO","location.state.code":"DC"}&pageSize=100
```

**Use Case**:
- Marketing campaigns by region
- Compliance reporting (GDPR by country)
- Regional pricing analysis
- Localization support

### 4. Multi-Tenant Data Isolation

**Scenario**: Ensure each tenant only sees their own data.

**Flow**:
1. User authenticated via OAuth2/OpenID Connect
2. JWT token includes `tenant_id` claim
3. Middleware extracts `X-Tenant` header
4. Repository filters all queries by `TenantId`
5. Commands include `IdUser` for audit trail

**Security**:
- All controllers require `[Authorize]` attribute
- `IUserContext` provides authenticated user and tenant ID
- Repository-level filtering prevents cross-tenant data access

### 5. Tenant Lifecycle Management

**Scenario**: Manage tenant from creation to deactivation.

**States**:
- **Active**: `IsActive=true, IsDeleted=false` - Normal operation
- **Inactive**: `IsActive=false, IsDeleted=false` - Suspended/paused
- **Deleted**: `IsActive=false, IsDeleted=true` - Soft deleted

**Operations**:
- Activate: Update `IsActive=true`
- Suspend: Update `IsActive=false`
- Delete: Call `DELETE /api/tenant/{id}` (soft delete)

## 🏗️ Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────┐
│  Entrypoints Layer                              │
│  - REST API Controllers                         │
│  - gRPC Services                                │
│  - AsyncWorker Consumers                        │
│  - Swagger/OpenAPI                              │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│  Application Layer                              │
│  - Commands (CreateTenant, UpdateTenant, etc.)  │
│  - Queries (GetAllTenant, GetTenantById, etc.)  │
│  - CommandHandlers & QueryHandlers             │
│  - DTOs (TenantDto)                             │
│  - Mapster Configurations                       │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│  Domain Layer                                   │
│  - TenantAggregate (root)                       │
│  - Value Objects (License, TypeDocument)        │
│  - Domain Events (TenantCreated, etc.)          │
│  - Domain Guards & Validations                  │
│  - Business Rules                               │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│  Infrastructure Layer                           │
│  - MongoDB Repositories                         │
│  - RabbitMQ Event Bus                           │
│  - Redis Cache                                  │
│  - Vault Integration                            │
└─────────────────────────────────────────────────┘
```

### CQRS Pattern

**Commands** (Write operations):
- `CreateTenantCommand` → `CreateTenantCommandHandler`
- `UpdateTenantCommand` → `UpdateTenantCommandHandler`
- `DeleteTenantCommand` → `DeleteTenantCommandHandler`

**Queries** (Read operations):
- `GetAllTenantQuery` → `GetAllTenantQueryHandler`
- `GetTenantByIdQuery` → `GetTenantByIdQueryHandler`
- `ExistTenantByIdQuery` → `ExistTenantByIdQueryHandler`

**Benefits**:
- Separation of read/write concerns
- Optimized queries without domain logic
- Scalable read/write paths
- Clear responsibility boundaries

### Domain Events

| Event | Trigger | Payload |
|-------|---------|---------|
| `TenantCreatedDomainEvent` | Tenant created | Full tenant data + CreatedBy |
| `TenantUpdatedDomainEvent` | Tenant updated | Updated tenant data + UpdatedBy |
| `TenantDeletedDomainEvent` | Tenant deleted | Tenant ID + DeletedBy |
| `TenantLicenseUpdatedDomainEvent` | License changed | Tenant ID + new License + UpdatedBy |
| `TenantLocationUpdatedDomainEvent` | Location changed | Tenant ID + new Location + UpdatedBy |

**Event Flow**:
1. Aggregate method called (e.g., `TenantAggregate.Create()`)
2. Domain event added to aggregate's event list
3. Handler commits aggregate to repository
4. Repository publishes events to RabbitMQ
5. Downstream consumers process events

### Project Structure

```
CodeDesignPlus.Net.Microservice.Tenants/
├── src/
│   ├── domain/
│   │   ├── CodeDesignPlus.Net.Microservice.Tenants.Domain/
│   │   │   ├── TenantAggregate.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── License.cs
│   │   │   │   └── TypeDocument.cs
│   │   │   ├── DomainEvents/
│   │   │   │   ├── TenantCreatedDomainEvent.cs
│   │   │   │   ├── TenantUpdatedDomainEvent.cs
│   │   │   │   └── ...
│   │   │   └── Errors.cs
│   │   ├── CodeDesignPlus.Net.Microservice.Tenants.Application/
│   │   │   ├── Tenant/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateTenant/
│   │   │   │   │   │   ├── CreateTenantCommand.cs
│   │   │   │   │   │   └── CreateTenantCommandHandler.cs
│   │   │   │   │   ├── UpdateTenant/
│   │   │   │   │   └── DeleteTenant/
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetAllTenant/
│   │   │   │   │   ├── GetTenantById/
│   │   │   │   │   └── ExistTenantById/
│   │   │   │   └── DataTransferObjects/
│   │   │   │       └── TenantDto.cs
│   │   │   └── Setup/
│   │   │       └── MapsterConfig.cs
│   │   └── CodeDesignPlus.Net.Microservice.Tenants.Infrastructure/
│   │       ├── Repositories/
│   │       ├── EventBus/
│   │       └── Cache/
│   └── entrypoints/
│       ├── CodeDesignPlus.Net.Microservice.Tenants.Rest/
│       │   ├── Controllers/
│       │   │   └── TenantController.cs
│       │   ├── Program.cs
│       │   ├── appsettings.json
│       │   └── Dockerfile
│       ├── CodeDesignPlus.Net.Microservice.Tenants.gRpc/
│       │   ├── Services/
│       │   │   └── TenantService.cs
│       │   ├── Protos/
│       │   │   └── tenant.proto
│       │   ├── Program.cs
│       │   └── Dockerfile
│       └── CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker/
│           ├── Consumers/
│           │   └── CreateTenantHandler.cs
│           ├── DomainEvents/
│           │   └── OrderPaidAndReadyForProvisioningDomainEvent.cs
│           ├── Program.cs
│           └── Dockerfile
├── tests/
│   ├── unit/
│   │   ├── Domain.Test/
│   │   ├── Application.Test/
│   │   ├── Rest.Test/
│   │   ├── gRpc.Test/
│   │   └── AsyncWorker.Test/
│   ├── integration/
│   │   ├── Rest.Test/
│   │   ├── gRpc.Test/
│   │   └── AsyncWorker.Test/
│   └── load/
│       └── load-rest.js (k6)
├── charts/
│   ├── ms-tenants-rest/
│   ├── ms-tenants-grpc/
│   └── ms-tenants-worker/
└── tools/
    ├── update-packages/
    ├── upgrade-dotnet/
    └── vault/
```

## 🧪 Testing

### Run All Tests

```bash
# Unit tests
dotnet test --filter FullyQualifiedName~Unit

# Integration tests
dotnet test --filter FullyQualifiedName~Integration

# All tests
dotnet test
```

### Unit Tests

**Coverage**:
- Domain Aggregate: `TenantAggregate` business logic
- Value Objects: `License`, `TypeDocument` validation
- Command Handlers: `CreateTenantCommandHandler`, etc.
- Query Handlers: `GetAllTenantQueryHandler`, etc.
- Controllers: `TenantController` endpoint routing
- gRPC Services: `TenantService` method mapping

**Example**:
```bash
dotnet test tests/unit/CodeDesignPlus.Net.Microservice.Tenants.Domain.Test/
```

### Integration Tests

**Coverage**:
- REST API endpoints with WebApplicationFactory
- gRPC service calls with TestServer
- AsyncWorker event consumption
- MongoDB repository operations
- RabbitMQ event publishing

**Example**:
```bash
dotnet test tests/integration/CodeDesignPlus.Net.Microservice.Tenants.Rest.Test/
```

### Load Testing (k6)

Test REST API under load.

**Run**:
```bash
k6 run tests/load/load-rest.js
```

**Scenarios**:
- Create tenant: 10 VUs, 1 minute
- Get all tenants: 20 VUs, 2 minutes
- Get by ID: 30 VUs, 2 minutes
- Update tenant: 5 VUs, 1 minute
- Delete tenant: 5 VUs, 30 seconds

**Thresholds**:
- 95% of requests < 500ms
- Error rate < 1%

### Test Data

**Valid Tenant**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Test Corporation",
  "typeDocument": { "code": "NIT", "name": "Número de Identificación Tributaria" },
  "numberDocument": "900123456-7",
  "phone": "+573001234567",
  "email": "test@example.com",
  "domain": "https://test.com",
  "location": { /* valid location */ },
  "license": {
    "id": "lic-test",
    "name": "Test License",
    "startDate": "2026-01-01T00:00:00Z",
    "endDate": "2026-12-31T23:59:59Z",
    "metadata": { "MaxUsers": "10" }
  },
  "isActive": true
}
```

## 🎯 Best Practices

### 1. Phone Number Validation

Always use E.164 format for phone numbers.

**Valid**: `+573001234567` (Colombia)  
**Invalid**: `300 123 4567`, `3001234567`

**Validation Regex**: `^\+?[1-9]\d{1,14}$`

### 2. License Management

**Best Practices**:
- Set `StartDate` to current or future date
- Set `EndDate` at least 1 day after `StartDate`
- Use `Metadata` dictionary for license limits (MaxUsers, MaxProjects, etc.)
- Validate license before granting tenant access

**Example**:
```csharp
var license = License.Create(
    Guid.NewGuid(),
    "Pro Plan",
    Instant.FromDateTimeUtc(DateTime.UtcNow),
    Instant.FromDateTimeUtc(DateTime.UtcNow.AddYears(1)),
    new Dictionary<string, string> {
        { "MaxUsers", "100" },
        { "MaxProjects", "50" },
        { "StorageGB", "500" }
    }
);
```

### 3. Location Data

**Complete Hierarchy**:
- Always provide Country (required)
- Include State, City, Locality, Neighborhood (optional but recommended)
- Use ISO codes for Country (Alpha2, Alpha3) and State (ISO 3166-2)
- Include Timezone for time-sensitive operations

### 4. Multi-Tenancy

**Tenant Isolation**:
- Use `X-Tenant` header in all requests
- Validate tenant context in middleware
- Filter all queries by `TenantId`
- Include `IdUser` in commands for audit trail

### 5. Error Handling

**RFC 7807 Problem Details**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Tenant with ID '550e8400-e29b-41d4-a716-446655440000' not found.",
  "instance": "/api/tenant/550e8400-e29b-41d4-a716-446655440000",
  "traceId": "00-abc123-def456-00"
}
```

### 6. Soft Delete

**Implementation**:
- Set `IsDeleted=true` and `IsActive=false`
- Set `DeletedAt` timestamp
- Set `DeletedBy` user ID
- Keep data for audit/recovery
- Filter queries to exclude deleted tenants

### 7. Domain Events

**Publishing**:
- Always publish domain events after aggregate changes
- Include full context in event payload
- Use idempotent event handlers
- Log event publishing/consumption

## 🔧 Troubleshooting

### Issue: Tenant Not Found After Creation

**Symptoms**: `GET /api/tenant/{id}` returns 404 immediately after `POST /api/tenant`

**Possible Causes**:
- Eventual consistency (MongoDB replication lag)
- Cache miss (Redis not updated)
- Incorrect tenant ID
- Tenant created in different tenant context

**Solution**:
1. Verify tenant ID matches
2. Check MongoDB for tenant document
3. Clear Redis cache: `redis-cli FLUSHDB`
4. Verify `X-Tenant` header matches tenant context

### Issue: Phone Validation Failed

**Symptoms**: `400 Bad Request` with error "PhoneTenantIsInvalid"

**Cause**: Phone number not in E.164 format

**Solution**:
```
❌ Invalid: "300 123 4567", "3001234567", "(300) 123-4567"
✅ Valid: "+573001234567", "+12025551234"
```

### Issue: License Start Date Greater Than End Date

**Symptoms**: `400 Bad Request` with error "LicenseStartDateGreaterThanEndDate"

**Solution**:
- Ensure `StartDate` < `EndDate`
- Use UTC timestamps (Instant type)
- Validate dates before sending request

### Issue: gRPC Connection Failed

**Symptoms**: "Connection refused" or "No connection could be made"

**Solution**:
1. Verify gRPC server is running: `dotnet run --project src/entrypoints/CodeDesignPlus.Net.Microservice.Tenants.gRpc`
2. Check port 5001 is not in use: `netstat -an | grep 5001`
3. Verify gRPC endpoint in client: `http://localhost:5001` or `https://localhost:5001`
4. Enable gRPC reflection for dynamic discovery

### Issue: AsyncWorker Not Consuming Events

**Symptoms**: Tenant not created after payment event published

**Possible Causes**:
- RabbitMQ connection failed
- Queue not declared
- Event routing key mismatch
- Consumer not registered

**Solution**:
1. Check RabbitMQ connection: `docker logs ms-tenants-worker`
2. Verify queue exists in RabbitMQ UI: `http://localhost:15672`
3. Check routing key: `TenantAggregate.CreateTenantHandler`
4. Restart AsyncWorker: `docker restart ms-tenants-worker`

### Issue: Vault Secrets Not Loading

**Symptoms**: MongoDB connection failed, "Username and password required"

**Possible Causes**:
- Vault not running
- Vault token expired
- Vault path incorrect
- Vault configuration disabled

**Solution**:
1. Verify Vault is running: `docker ps | grep vault`
2. Check Vault token: `vault login <token>`
3. Verify secrets exist: `vault kv get secret/security-codedesignplus/ms-tenants/mongo`
4. Enable Vault in appsettings: `"Vault": { "Enable": true }`
5. Fallback to appsettings for local dev: Set connection strings directly

### Issue: RBAC Validation Failed

**Symptoms**: `403 Forbidden` even with valid JWT token

**Possible Causes**:
- RBAC service not running
- User role not assigned
- RBAC validation enabled without configuration

**Solution**:
1. Verify RBAC service: `curl http://ms-rbac:5001/health`
2. Check user roles in JWT token claims
3. Disable RBAC for local dev: `"Security": { "ValidateRbac": false }`
4. Assign role to user via RBAC API

### Issue: MongoDB Connection Pool Exhausted

**Symptoms**: Timeouts, "Connection pool exhausted"

**Solution**:
1. Increase connection pool size in MongoDB connection string
2. Reduce request rate (implement rate limiting)
3. Optimize queries (add indexes)
4. Scale MongoDB replicas
5. Monitor connection metrics with MongoDB Atlas

## 🌍 Multi-Tenancy Strategy

### Tenant Isolation Approaches

#### 1. Database-per-Tenant
**Not Used**: Each tenant has separate MongoDB database.

**Pros**: Complete isolation, simple backups  
**Cons**: Expensive, complex management

#### 2. Collection-per-Tenant
**Not Used**: Each tenant has separate collection in shared database.

**Pros**: Moderate isolation  
**Cons**: Schema migrations complex, limited scalability

#### 3. Row-Level Isolation (Current Implementation)
**Used**: All tenants share same collection, filtered by `TenantId`.

**Pros**: Simple, scalable, cost-effective  
**Cons**: Requires careful query filtering, cross-tenant leak risk

### Implementation

**Tenant Context Middleware**:
```csharp
public class TenantContextMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = context.Request.Headers["X-Tenant"].FirstOrDefault();
        if (!string.IsNullOrEmpty(tenantId))
        {
            context.Items["TenantId"] = Guid.Parse(tenantId);
        }
        await _next(context);
    }
}
```

**Repository Filtering**:
```csharp
public async Task<TenantAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
{
    var tenantId = _userContext.TenantId; // From JWT claims
    return await _collection
        .Find(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted)
        .FirstOrDefaultAsync(cancellationToken);
}
```

### Security Considerations

- **Always filter by TenantId**: Never trust client input alone
- **Validate JWT claims**: Ensure `tenant_id` claim matches `X-Tenant` header
- **Audit logging**: Log all tenant context changes
- **Cross-tenant testing**: Verify queries don't leak data between tenants

## 🔒 Data Isolation

### MongoDB Indexing

**Required Indexes**:
```javascript
// Compound index for tenant isolation
db.tenants.createIndex({ "tenantId": 1, "id": 1 });

// Query optimization
db.tenants.createIndex({ "tenantId": 1, "isDeleted": 1, "isActive": 1 });
db.tenants.createIndex({ "tenantId": 1, "email": 1 }, { unique: true });
db.tenants.createIndex({ "tenantId": 1, "numberDocument": 1 }, { unique: true });

// License expiration queries
db.tenants.createIndex({ "tenantId": 1, "license.endDate": 1 });
```

### Unique Constraints

**Per-Tenant Uniqueness**:
- Email: Unique within tenant context
- NumberDocument: Unique within tenant context
- Domain: Optional, unique if provided

**Global Uniqueness**:
- Tenant ID: Globally unique (GUID)

### Cache Strategy

**Redis Keys**:
```
tenant:{tenantId}:{id}              → Full tenant object
tenant:{tenantId}:list:{page}       → Paginated list
tenant:{tenantId}:email:{email}     → Email lookup
tenant:{tenantId}:document:{number} → Document lookup
```

**Cache Invalidation**:
- On create: Invalidate list cache
- On update: Invalidate specific tenant + list cache
- On delete: Invalidate specific tenant + list cache

**TTL**: 5 minutes (configurable via `RedisCache:Expiration`)

## 🚀 Event-Driven Provisioning

### Automatic Tenant Creation Flow

```
┌───────────────┐
│  Payments MS  │
│               │
│ Order paid    │
│ and ready     │
└───────┬───────┘
        │
        │ Publish
        ▼
┌─────────────────────────────────────┐
│  RabbitMQ Exchange                  │
│  Exchange: TenantAggregate          │
│  Routing Key: CreateTenantHandler   │
└───────┬─────────────────────────────┘
        │
        │ Route
        ▼
┌─────────────────────────────────────┐
│  Tenants AsyncWorker                │
│  Queue: CreateTenantHandler         │
│                                     │
│  ┌────────────────────────────┐    │
│  │ CreateTenantHandler        │    │
│  │ (Consumer)                 │    │
│  │                            │    │
│  │ 1. Parse event             │    │
│  │ 2. Map to CreateTenant     │    │
│  │    Command                 │    │
│  │ 3. Send via Mediator       │    │
│  └────────────────────────────┘    │
└───────┬─────────────────────────────┘
        │
        │ Execute
        ▼
┌─────────────────────────────────────┐
│  CreateTenantCommandHandler         │
│                                     │
│  1. Validate command                │
│  2. Create TenantAggregate          │
│  3. Save to MongoDB                 │
│  4. Publish TenantCreated event     │
└───────┬─────────────────────────────┘
        │
        │ Publish
        ▼
┌─────────────────────────────────────┐
│  RabbitMQ Exchange                  │
│  Event: TenantCreatedDomainEvent    │
└───────┬─────────────────────────────┘
        │
        │ Consume
        ▼
┌─────────────────────────────────────┐
│  Downstream Services                │
│  - Users MS (create admin user)     │
│  - RBAC MS (create default roles)   │
│  - Notification MS (welcome email)  │
└─────────────────────────────────────┘
```

### Event Payload

**OrderPaidAndReadyForProvisioningDomainEvent**:
```json
{
  "aggregateId": "payment-guid",
  "tenantDetail": {
    "id": "tenant-guid",
    "name": "Acme Corporation",
    "typeDocument": { "code": "NIT", "name": "NIT" },
    "numberDocument": "900123456-7",
    "web": "https://acme.com",
    "phone": "+573001234567",
    "email": "contact@acme.com",
    "location": { /* ... */ }
  },
  "license": {
    "id": "lic-guid",
    "name": "Pro Plan",
    "startDate": "2026-01-01T00:00:00Z",
    "endDate": "2026-12-31T23:59:59Z",
    "metadata": { "MaxUsers": "100" }
  },
  "buyerId": "user-guid",
  "occurredAt": "2026-05-15T12:00:00Z"
}
```

### Idempotency

**Duplicate Event Handling**:
- Check if tenant with ID already exists
- If exists, log and skip creation
- Use GUID from payment event as tenant ID
- Prevents duplicate tenants from event replays

## 🔐 Security

### Authentication & Authorization

**JWT Bearer Token**:
- All endpoints require `[Authorize]` attribute
- Token validated against configured issuer
- Token lifetime validated

**Required Claims**:
- `sub` (subject): User ID
- `tenant_id`: Tenant ID for multi-tenancy
- `role`: User role (e.g., "Admin", "User")

### RBAC Integration

**Role-Based Access Control**:
- Integrated with `ms-rbac` microservice
- Validates user permissions before command execution
- Configurable via `Security:ValidateRbac`

**Example**:
```csharp
[Authorize(Policy = "Tenants.Create")]
public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto data)
{
    // Only users with "Tenants.Create" permission can execute
}
```

### License Validation

**License Check**:
- Configurable via `Security:ValidateLicense`
- Validates tenant's license is active (current date between StartDate and EndDate)
- Checks license limits (MaxUsers, MaxProjects, etc.)

### Sensitive Data

**No Sensitive Data Stored**:
- No credit card information
- No passwords
- No PII beyond contact info

**PII Data**:
- Email, phone, address (required for business operations)
- Encrypted in transit (HTTPS)
- Access logged for audit

### Vault Integration

**Secret Management**:
- MongoDB credentials stored in Vault
- RabbitMQ credentials stored in Vault
- Secrets rotated regularly
- Fallback to appsettings for local dev

## ❓ FAQ

### Q: Can a tenant have multiple licenses?
**A**: No, each tenant has one active license at a time. Update the license via `UpdateTenant` to change it.

### Q: What happens when a license expires?
**A**: The microservice doesn't automatically deactivate tenants. Implement a scheduled job to check `license.endDate` and deactivate expired tenants, or rely on downstream services (RBAC, Security) to validate license before granting access.

### Q: Can I change a tenant's ID after creation?
**A**: No, tenant IDs are immutable. If you need to change it, create a new tenant and migrate data.

### Q: How do I delete a tenant permanently?
**A**: The microservice uses soft delete. For permanent deletion, manually delete the MongoDB document. This is not recommended due to referential integrity concerns.

### Q: Can I bulk import tenants?
**A**: The API doesn't have a bulk endpoint. Use the REST API in a loop or implement a custom import script that calls `CreateTenantCommand`.

### Q: What document types are supported?
**A**: Any document type can be used. Common types: NIT, RUT, CC, CE, Passport. The `Code` field is limited to 3 characters.

### Q: How do I query tenants by location?
**A**: Use the `filters` query parameter:
```http
GET /api/tenant?filters={"location.country.alpha2":"CO"}
GET /api/tenant?filters={"location.state.code":"DC","location.city.name":"Bogotá"}
```

### Q: Can I use the microservice without Vault?
**A**: Yes, set `"Vault": { "Enable": false }` and provide connection strings directly in appsettings.json for local development.

### Q: How do I integrate with the AsyncWorker?
**A**: Publish `OrderPaidAndReadyForProvisioningDomainEvent` to RabbitMQ with exchange `TenantAggregate` and routing key `CreateTenantHandler`. The worker will consume and create the tenant.

### Q: What happens if tenant creation fails in AsyncWorker?
**A**: The event is requeued to RabbitMQ (with dead-letter queue for permanent failures). Monitor RabbitMQ for failed messages and logs for error details.

## 🤝 Contributing

Please read our Contributing Guide for details on our code of conduct and development process.

**Development Workflow**:
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m 'Add amazing feature'`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

**Code Standards**:
- Follow Clean Architecture principles
- Write unit tests for all business logic
- Write integration tests for API endpoints
- Use meaningful commit messages
- Update documentation (README, Swagger, etc.)

## 📄 License

This project is licensed under the GNU Lesser General Public License v3.0 - see the [LICENSE.md](LICENSE.md) file for details.

## 🔧 Tools

The repository includes several utility scripts in the `tools/` directory:

- `convert-crlf-to-lf.sh`: Converts line endings from CRLF to LF
- `update-packages/`: Updates NuGet packages to latest versions
- `upgrade-dotnet/`: Upgrades the solution to a newer .NET version
- `vault/`: Vault configuration scripts for secret management
- `sonarqube/`: SonarQube analysis configuration

## 📚 Additional Resources

- **CodeDesignPlus SDK**: [Documentation Site](https://codedesignplus.github.io/)
- **Docker Compose Environment**: [CodeDesignPlus.Environment.Dev](https://github.com/codedesignplus/CodeDesignPlus.Environment.Dev)
- **Swagger UI**: `http://localhost:5000/ms-tenants/swagger` (when running locally)
- **gRPC Reflection**: Use tools like [grpcurl](https://github.com/fullstorydev/grpcurl) or [BloomRPC](https://github.com/bloomrpc/bloomrpc)

## 🐳 Docker Support

### Build Docker Images

**REST API**:
```bash
docker build -t ms-tenants-rest:latest . -f src/entrypoints/CodeDesignPlus.Net.Microservice.Tenants.Rest/Dockerfile
```

**gRPC**:
```bash
docker build -t ms-tenants-grpc:latest . -f src/entrypoints/CodeDesignPlus.Net.Microservice.Tenants.gRpc/Dockerfile
```

**AsyncWorker**:
```bash
docker build -t ms-tenants-worker:latest . -f src/entrypoints/CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker/Dockerfile
```

### Run Docker Containers

```bash
# Create backend network
docker network create backend

# Run REST API
docker run -d -p 5000:5000 --network=backend \
  -e ASPNETCORE_ENVIRONMENT=Docker \
  -e MONGO__ENABLE=true \
  -e MONGO__DATABASE=db-ms-tenants \
  -e RABBITMQ__ENABLE=true \
  -e RABBITMQ__HOST=rabbitmq \
  -e REDIS__INSTANCES__CORE__CONNECTIONSTRING=redis-core:6379 \
  --name ms-tenants-rest \
  ms-tenants-rest:latest

# Run gRPC
docker run -d -p 5001:5001 --network=backend \
  -e ASPNETCORE_ENVIRONMENT=Docker \
  --name ms-tenants-grpc \
  ms-tenants-grpc:latest

# Run AsyncWorker
docker run -d --network=backend \
  -e ASPNETCORE_ENVIRONMENT=Docker \
  --name ms-tenants-worker \
  ms-tenants-worker:latest
```

## ☸️ Kubernetes Deployment

### Helm Charts

The microservice includes Helm charts for Kubernetes deployment.

**Deploy to Staging**:
```bash
helm upgrade --install ms-tenants-rest ./charts/ms-tenants-rest \
  -f ./charts/ms-tenants-rest/Staging.yaml \
  -n codedesignplus --create-namespace

helm upgrade --install ms-tenants-grpc ./charts/ms-tenants-grpc \
  -f ./charts/ms-tenants-grpc/Staging.yaml \
  -n codedesignplus

helm upgrade --install ms-tenants-worker ./charts/ms-tenants-worker \
  -f ./charts/ms-tenants-worker/Staging.yaml \
  -n codedesignplus
```

**Deploy to Production**:
```bash
helm upgrade --install ms-tenants-rest ./charts/ms-tenants-rest \
  -f ./charts/ms-tenants-rest/Production.yaml \
  -n codedesignplus
```

### Service Endpoints

**Staging**:
- REST: `http://ms-tenants-rest.codedesignplus.svc.cluster.local:5000`
- gRPC: `http://ms-tenants-grpc.codedesignplus.svc.cluster.local:5001`
- Ingress: `https://staging.example.com/ms-tenants`

**Production**:
- REST: `http://ms-tenants-rest.codedesignplus.svc.cluster.local:5000`
- gRPC: `http://ms-tenants-grpc.codedesignplus.svc.cluster.local:5001`
- Ingress: `https://api.example.com/ms-tenants`

---

**Built with ❤️ by CodeDesignPlus**  
For support, contact: support@codedesignplus.com
