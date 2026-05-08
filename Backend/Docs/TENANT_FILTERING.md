# Tenant Filtering

Backend tenant filtering has one default rule: all `BaseEntity` queries rely on the `DatabaseContext` global query filter for tenant isolation and soft delete.

The global filter is:

```csharp
!IsDeleted && (IsSuperAdmin || TenantId == CurrentTenantId)
```

## Rules

- Do not add explicit `TenantId == ...` or `!IsDeleted` filters in normal application handlers. The global query filter owns those constraints.
- Add explicit `RestaurantId` filtering only when data is scoped below the tenant to a specific restaurant.
- Use `WhereCurrentRestaurant(tenantContext)` for entities that expose `RestaurantId`.
- For entities without a direct `RestaurantId`, filter through the owning relationship. Example: `DiningTable` and `TableReservation` are restaurant-scoped through `TableLayout.RestaurantId`.
- Do not call `IgnoreQueryFilters()` in normal tenant-scoped handlers.

## Allowed `IgnoreQueryFilters()` Uses

`IgnoreQueryFilters()` is allowed only when a tenant context is not available or the operation is intentionally system-wide:

- Superadmin/system tenant activation flows.
- Public tenant/domain resolution before authentication.
- Public reservation creation before authentication, with explicit tenant and restaurant validation.
- Runtime seeders that reconcile demo/system data.
- Identity/profile repair where existing records must be found regardless of the current request tenant.

Every production `IgnoreQueryFilters()` call must include a nearby comment explaining why bypassing the global filter is required.

## DbContext Pooling Note

The global query filter must reference runtime `DatabaseContext` properties, not tenant values evaluated once during model creation. If `AddDbContextPool` is introduced later, also revisit how per-request tenant state is injected into `DatabaseContext`, because pooled contexts can retain constructor-injected state across requests unless the state is reset explicitly.
