# Biteflow - Restaurant Management Platform

![Angular](https://img.shields.io/badge/Angular-20-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-68217A?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-Realtime-0A7ACC?style=for-the-badge)

---

Biteflow is a full-stack restaurant management web application built with Angular and ASP.NET Core Web API.
It supports restaurant onboarding, role-based dashboards, menu and inventory management, staff administration, table reservations, order workflows, notifications, and analytics.

---

## Technologies Used

### Frontend (Angular)

- Angular 20+
- TypeScript
- RxJS
- Angular Material
- Reactive Forms
- Route Guards
- OAuth2 / OIDC client authentication
- SignalR client for realtime updates
- Chart.js for analytics views

### Backend (ASP.NET Core)

- ASP.NET Core Web API (.NET 8)
- Entity Framework Core Code-First
- SQL Server
- ASP.NET Core Identity
- Duende IdentityServer
- MediatR
- FluentValidation
- Serilog
- SignalR hubs

### Infrastructure & Tools

- Azure Blob Storage integration
- hCaptcha verification
- Swagger / OpenAPI
- EF Core migrations
- xUnit integration and unit tests
- Azure Pipelines configuration
- Git and GitHub

---

## Key Features

- Restaurant tenant activation and onboarding workflow
- Role-based authentication for superadmin, admin, waiter, kitchen staff, and customers
- Admin CRUD screens for meals, meal categories, inventory, tables, reservations, and staff
- Waiter order creation and table workflow
- Kitchen order status workflow with realtime updates
- Realtime notifications through SignalR
- Table layout and reservation management
- Analytics and KPI endpoints for restaurant reporting
- Secure staff identity lifecycle management
- Environment-based configuration for email, captcha, blob storage, and seed users

---

## Application Areas

### Public Portal

Public restaurant-facing pages for tenant activation, customer registration, and table reservation entry points.

### Admin Dashboard

Restaurant administration area for managing staff, menus, meals, inventory, table layouts, reservations, and reporting.

### Waiter Screen

Operational waiter workflow for table selection, order creation, and order tracking.

### Kitchen Screen

Realtime kitchen workflow for receiving orders and updating preparation status.

### Superadmin Panel

Platform-level activation request review and tenant management workflow.

---

## Running the Project Locally

### Backend (API)

1. Navigate to the repository root.
2. Restore and build the backend:

```powershell
dotnet restore Backend/Market.Backend.sln
dotnet build Backend/Market.Backend.sln
```

3. Configure `Backend/Market.API/appsettings.json` or environment variables. At minimum, set:

```powershell
ConnectionStrings__Main
ActivationLink__TokenSecret
Captcha__SecretKey
AzureBlobStorage__ConnectionString
AzureBlobStorage__ContainerName
```

4. Run the EF Core migrations:

```powershell
dotnet ef database update --context IdentityDatabaseContext --project Backend/Market.Infrastructure --startup-project Backend/Market.API
dotnet ef database update --context DatabaseContext --project Backend/Market.Infrastructure --startup-project Backend/Market.API
```

5. Start the API:

```powershell
dotnet run --project Backend/Market.API
```

6. In development, Swagger is available at `/swagger`.

### Frontend (Angular)

1. Navigate to the frontend project:

```powershell
cd Frontend
```

2. Install dependencies and start the Angular dev server:

```powershell
npm install
npm start
```

3. The frontend will be available at `http://localhost:4200`.

---

## Build and Test

### Backend Tests

```powershell
dotnet test Backend/Market.Tests/Market.Tests.csproj
```

### Frontend Build

```powershell
cd Frontend
npm run build
```

---

## Project Structure Overview

```text
Backend/
  Market.API/             # ASP.NET Core API, controllers, auth, SignalR hubs
  Market.Application/     # CQRS commands, queries, validators, application logic
  Market.Domain/          # Domain entities and shared domain model
  Market.Infrastructure/  # EF Core contexts, migrations, persistence, seeders
  Market.Shared/          # Shared constants and options
  Market.Tests/           # Backend unit and integration tests

Frontend/
  src/app/modules/        # Angular feature modules and screens
  src/app/endpoints/      # API endpoint wrappers
  src/app/services/       # Auth, settings, notifications, and shared services
```

---

## Deployment

This project can be deployed using:

- Azure App Service or another ASP.NET Core hosting environment
- SQL Server for production data
- Azure Blob Storage for uploaded files
- Environment variables or a secret store for production configuration
- Azure Pipelines or GitHub Actions for CI/CD

For staging and production, replace the development IdentityServer signing credential strategy with a persisted certificate or key.

---

## Additional Documentation

- [Backend setup checklist](Backend/README.md)
- [Demo users and roles](Backend/Docs/DEMO_USERS.md)
- [Tenant filtering rules](Backend/Docs/TENANT_FILTERING.md)

---

## About the Developer

This project was built as a full-stack .NET and Angular restaurant management system.
It demonstrates practical work with clean architecture, REST APIs, Identity/OIDC authentication, tenant-scoped data, realtime workflows, and Angular dashboard development.

GitHub: [github.com/AnurZ](https://github.com/AnurZ)  
LinkedIn: [linkedin.com/in/anur-zjakić-0169a5309](https://www.linkedin.com/in/anur-zjaki%C4%87-0169a5309/)

---

## License

This repository is for educational and portfolio purposes.
Review configuration, security, signing credentials, secrets, and deployment settings before using it in production.
