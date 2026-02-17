# Demo Users And Roles

This project seeds two important demo identity users in `IdentitySeeder`.

## 1) Platform Superadmin

- Username: `superadmin`
- Password: `superadmin`
- Email: `superadmin@demo.local`
- Roles:
  - `superadmin`
- Notes:
  - Seeder enforces this user as the only account with `superadmin`.
  - If any other user has `superadmin`, that role is removed from them.

## 2) Demo App User

- Username: `string`
- Password: `string`
- Email: `string@legacy.local` (identity email format)
- Roles:
  - `admin`
  - `staff`
  - `customer`
- Notes:
  - Seeder explicitly removes `superadmin` from this user.
  - Use this account for app/demo flows that should not have platform-owner rights.

## SeedAdmin Config User (appsettings)

- Source: `SeedAdmin` section in `Backend/Market.API/appsettings.json`
- Behavior:
  - Seeded only if `Email` and `Password` are provided.
  - Gets `admin` role.
  - Is explicitly stripped of `superadmin` role.

## Restaurant Activation Admin Convention (Project Note)

- When a new restaurant is activated, create one tenant admin login account.
- Username/email format:
  - `restaurantname.admin@biteflow.com`
- Password format:
  - `restaurantnamefirstpassword`
- Role assignment:
  - `admin`
- Scope:
  - User must be assigned to the activated restaurant tenant (`TenantId`).
