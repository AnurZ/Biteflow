# Demo Users And Roles

This project seeds these demo identity users in `IdentitySeeder`.

## 1) Platform Superadmin

- Username: `superadmin`
- Password: `Superadmin1!`
- Email: `superadmin@demo.local`
- Roles:
  - `superadmin`
- Notes:
  - Seeder enforces this user as the only account with `superadmin`.
  - If any other user has `superadmin`, that role is removed from them.

## 2) Demo Admin App User

- Username: `string`
- Password: `StringUser1!`
- Email: `string@legacy.local` (identity email format)
- Roles:
  - `admin`
  - `staff`
  - `customer`
- Notes:
  - Seeder explicitly removes `superadmin` from this user.
  - Use this account for app/demo flows that should not have platform-owner rights.

## 3) Demo Waiter

- Username: `waiter1`
- Password: `WaiterUser1!`
- Email: `waiter1@legacy.local` (identity email format)
- Roles:
  - `waiter`
- Notes:
  - Use this account for waiter-scoped app/demo flows.

## 4) Demo Kitchen User

- Username: `kitchen1`
- Password: `KitchenUser1!`
- Email: `kitchen1@legacy.local` (identity email format)
- Roles:
  - `kitchen`
- Notes:
  - Use this account for kitchen-scoped app/demo flows.

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
- Password setup:
  - No predictable or plaintext password is returned or emailed.
  - The owner receives a short-lived, one-time password setup link by email after activation.
- Role assignment:
  - `admin`
- Scope:
  - User must be assigned to the activated restaurant tenant (`TenantId`).
