# Khan Bahadur Restora

This repository contains the active Khan Bahadur Restora system:

- `frontend`: React + Vite
- `backend`: ASP.NET Core 8 Web API + EF Core + PostgreSQL

The backend has been migrated from SQL Server to PostgreSQL. The repo now uses the PostgreSQL EF Core provider, a PostgreSQL design-time context factory, and a repo-local `dotnet-ef` tool manifest for repeatable migrations.

## What Changed

- SQL Server provider removed from the active backend project
- PostgreSQL provider added with `UseNpgsql()`
- Runtime database setup switched from SQL Server configuration to PostgreSQL connection strings
- Design-time EF context factory switched to PostgreSQL
- Initial PostgreSQL migration generated in `backend/Migrations`
- Monthly report date handling fixed so PostgreSQL UTC timestamp comparisons stay valid

## Seeded Accounts

When the database is empty, these accounts are seeded automatically:

- `admin@restaurant.com` / `Admin@123`
- `manager@restaurant.com` / `Manager@123`
- `cashier@restaurant.com` / `Cashier@123`
- `chef@restaurant.com` / `Chef@123`
- `waiter@restaurant.com` / `Waiter@123`
- `customer@restaurant.com` / `Customer@123`

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- PostgreSQL running locally

## PostgreSQL Configuration

The backend reads the connection string from [appsettings.json](C:/Users/User/Downloads/RestaurantAppManager-master%20(1)/RestaurantAppManager-master/backend/appsettings.json).

Default template:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=khan_bahadur_restora_db;Username=postgres;Password=CHANGE_ME;Include Error Detail=true"
}
```

Replace `CHANGE_ME` with your real PostgreSQL password before running migrations or starting the API.

You can also override the connection string from PowerShell without editing the file:

```powershell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=khan_bahadur_restora_db;Username=postgres;Password=YOUR_REAL_PASSWORD;Include Error Detail=true"
```

## Backend Commands

Open PowerShell and run:

```powershell
cd "C:\Users\User\Downloads\RestaurantAppManager-master (1)\RestaurantAppManager-master\backend"
dotnet restore
dotnet build
dotnet tool restore
```

### Create A New Migration

```powershell
dotnet tool run dotnet-ef migrations add YourMigrationName
```

### Apply Migrations To PostgreSQL

```powershell
dotnet tool run dotnet-ef database update
```

### Run The Backend

```powershell
dotnet run
```

Backend URLs:

- HTTP Swagger UI: [http://localhost:5000/swagger](http://localhost:5000/swagger)
- HTTPS Swagger UI: [https://localhost:5001/swagger](https://localhost:5001/swagger)
- API base URL used by the frontend: [http://localhost:5000](http://localhost:5000)

## Frontend Commands

The frontend uses `VITE_API_BASE_URL=http://localhost:5000` from [frontend/.env](C:/Users/User/Downloads/RestaurantAppManager-master%20(1)/RestaurantAppManager-master/frontend/.env).

```powershell
cd "C:\Users\User\Downloads\RestaurantAppManager-master (1)\RestaurantAppManager-master\frontend"
npm install
npm run build
npm run dev
```

Frontend URL:

- [http://localhost:5173](http://localhost:5173)

## Recommended First Run

1. Update the PostgreSQL password in [appsettings.json](C:/Users/User/Downloads/RestaurantAppManager-master%20(1)/RestaurantAppManager-master/backend/appsettings.json) or set `ConnectionStrings__DefaultConnection`.
2. Run `dotnet restore`.
3. Run `dotnet tool restore`.
4. Run `dotnet tool run dotnet-ef database update`.
5. Run `dotnet run` and keep the backend on `http://localhost:5000` or `https://localhost:5001`.
6. Start the frontend with `npm run dev`.
7. Open the frontend on [http://localhost:5173](http://localhost:5173) and sign in with one of the seeded accounts.
8. If Vite says port `5173` is busy, stop the conflicting process instead of letting Vite switch to another port. The project is now intentionally pinned to `5173`.

## UI Test Checklist

### 1. Authentication

1. Open [http://localhost:5173/login](http://localhost:5173/login)
2. Sign in as `manager@restaurant.com`
3. Confirm login succeeds and the manager dashboard loads
4. Open register and confirm password show/hide works there as well

### 2. Customer Cart And Place Order

1. Sign in as `customer@restaurant.com`
2. Open the menu
3. Increase or decrease quantity from the item card
4. Click the one-line `Add to Cart` button
5. Open cart and confirm image, quantity, subtotal, and automatic total updates
6. Open `Place Order`
7. Choose `DineIn`, `TakeAway`, or `Delivery`
8. Submit the order
9. Confirm the customer dashboard and order history refresh with the new order

### 3. Create Order As Cashier Or Waiter

1. Sign in as `cashier@restaurant.com` or `waiter@restaurant.com`
2. Open the orders management page
3. Add one or more menu items
4. Select order type and table if needed
5. Click `Create Order`
6. Confirm the order appears in the queue and related `OrderItems` are saved

### 4. Record Payment

1. Sign in as `cashier@restaurant.com`, `manager@restaurant.com`, or `admin@restaurant.com`
2. Open orders management
3. Choose an unpaid order in the payment form
4. Enter amount and payment method
5. Save the payment
6. Confirm a `Payments` record is created and the order status updates after full payment

### 5. Manager Dashboard

1. Sign in as `manager@restaurant.com`
2. Open the manager dashboard
3. Confirm today’s totals, weekly sales, and top-selling items load
4. Create or pay an order in another tab
5. Return and confirm dashboard data refreshes

### 6. Reports

1. Sign in as `manager@restaurant.com` or `admin@restaurant.com`
2. Open reports
3. Switch between `Daily`, `Weekly`, and `Monthly`
4. Confirm revenue, order counts, average order value, top menu items, and employee performance all load

## Postman API Checks

Use the backend base URL:

```text
http://localhost:5000
```

### 1. Register

```http
POST /api/auth/register
Content-Type: application/json
```

```json
{
  "fullName": "Postman Customer",
  "email": "postman.customer@example.com",
  "password": "Customer@123",
  "phoneNumber": "01711111111",
  "address": "Dhaka"
}
```

### 2. Login

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "manager@restaurant.com",
  "password": "Manager@123"
}
```

Use the returned token as:

```text
Authorization: Bearer YOUR_TOKEN
```

### 3. Check Current User

```http
GET /api/auth/me
Authorization: Bearer YOUR_TOKEN
```

### 4. Fetch Categories And Menu

```http
GET /api/categories/active
Authorization: Bearer YOUR_TOKEN
```

```http
GET /api/menuitems?pageSize=10
Authorization: Bearer YOUR_TOKEN
```

Copy one menu item `id`.

### 5. Fetch Available Tables

```http
GET /api/tables/available?guests=1
Authorization: Bearer YOUR_TOKEN
```

Copy one table `id` for dine-in tests if needed.

### 6. Create Order

```http
POST /api/orders
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN
```

```json
{
  "tableId": "REPLACE_WITH_TABLE_ID_OR_NULL",
  "orderType": "DineIn",
  "notes": "PostgreSQL smoke test order",
  "items": [
    {
      "menuItemId": "REPLACE_WITH_MENU_ITEM_ID",
      "quantity": 2
    }
  ]
}
```

Expected result:

- `201 Created`
- created order payload includes items

### 7. Verify Orders

```http
GET /api/orders?pageSize=20
Authorization: Bearer YOUR_TOKEN
```

### 8. Record Payment

```http
POST /api/payments
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN
```

```json
{
  "orderId": "REPLACE_WITH_ORDER_ID",
  "amount": 25.50,
  "paymentMethod": "Cash",
  "transactionId": "POS-1001"
}
```

Expected result:

- `201 Created`
- `GET /api/payments?pageSize=20` returns the new payment
- `GET /api/orders/{orderId}` reflects updated payment status after full payment

### 9. Dashboard Endpoints

```http
GET /api/dashboard/admin
Authorization: Bearer YOUR_ADMIN_TOKEN
```

```http
GET /api/dashboard/manager
Authorization: Bearer YOUR_MANAGER_TOKEN
```

```http
GET /api/dashboard/customer
Authorization: Bearer YOUR_CUSTOMER_TOKEN
```

### 10. Report Endpoints

```http
GET /api/reports/sales-summary?period=daily
Authorization: Bearer YOUR_MANAGER_TOKEN
```

```http
GET /api/reports/top-menu-items?take=5
Authorization: Bearer YOUR_MANAGER_TOKEN
```

```http
GET /api/reports/employee-performance
Authorization: Bearer YOUR_MANAGER_TOKEN
```

### 11. Image Upload

```http
POST /api/uploads/image
Authorization: Bearer YOUR_TOKEN
Content-Type: multipart/form-data
```

Send a single file field named `file`. The response returns the saved image path.

## Verification Status

- `backend`: `dotnet build` passes with PostgreSQL provider installed
- `frontend`: `npm run build` already passes
- PostgreSQL migration files were generated successfully in [backend/Migrations](C:/Users/User/Downloads/RestaurantAppManager-master%20(1)/RestaurantAppManager-master/backend/Migrations)
- PostgreSQL migrations apply successfully with the configured local credentials
