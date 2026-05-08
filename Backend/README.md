# Biteflow Backend Checklist

## Required Configuration

- [ ] Set `ASPNETCORE_ENVIRONMENT` (`Development`, `Staging`, or `Production`).
- [ ] Set `ConnectionStrings__Main`.
- [ ] Set `IdentityServer__Authority` outside local development.
- [ ] Set `Authentication__Google__ClientId` and `Authentication__Google__ClientSecret`.
- [ ] Set `AzureBlobStorage__ConnectionString` and `AzureBlobStorage__ContainerName`.
- [ ] Set `Captcha__SecretKey`.
- [ ] Confirm `Captcha__VerifyEndpoint` (`https://hcaptcha.com/siteverify` by default).
- [ ] Confirm `Captcha__Enabled`.
- [ ] Set `ActivationLink__BaseUrl`, `ActivationLink__Route`, `ActivationLink__Lifetime`, and `ActivationLink__TokenSecret`.
- [ ] If email is enabled, set `Email__Enabled=true` plus `Email__Host`, `Email__Port`, `Email__EnableSsl`, `Email__FromAddress`, `Email__FromName`, `Email__Username`, and `Email__Password`.
- [ ] If a first admin should be seeded, set `SeedAdmin__Email`, `SeedAdmin__Password`, and optionally `SeedAdmin__DisplayName`.
- [ ] For staging/production, replace or persist the current IdentityServer `AddDeveloperSigningCredential()` signing key strategy.

## Manual Migrations

- [ ] Use the target environment and connection string before running EF commands.
- [ ] Run Identity migrations first:

```powershell
dotnet ef database update `
  --context IdentityDatabaseContext `
  --project Backend/Market.Infrastructure `
  --startup-project Backend/Market.API
```

- [ ] Package Manager Console equivalent:

```powershell
Update-Database -Context IdentityDatabaseContext -Project Market.Infrastructure -StartupProject Market.API
```

- [ ] Run application migrations second:

```powershell
dotnet ef database update `
  --context DatabaseContext `
  --project Backend/Market.Infrastructure `
  --startup-project Backend/Market.API
```

- [ ] Package Manager Console equivalent:

```powershell
Update-Database -Context DatabaseContext -Project Market.Infrastructure -StartupProject Market.API
```

## Seed Strategy

- [ ] Expect startup migrations outside `IntegrationTests` and `Testing`.
- [ ] Expect dynamic demo data only in `Development`.
- [ ] Expect Identity roles/users after migrations in every non-test environment.
- [ ] Use `SeedAdmin__Email` and `SeedAdmin__Password` to create the configured admin.
- [ ] Check [demo users](Docs/DEMO_USERS.md) for seeded demo credentials and roles.

## Run Locally

- [ ] Start the API:

```powershell
dotnet run --project Backend/Market.API
```

- [ ] In `Development`, open `/swagger`.

## Additional Docs

- [Tenant filtering rules](Docs/TENANT_FILTERING.md)
- [Demo users](Docs/DEMO_USERS.md)
