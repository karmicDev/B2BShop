# B2BShop

A personal learning project: a B2B shop with an ASP.NET Core API, with a Blazor web frontend and an Android app planned for later.

## Current status

- ASP.NET Core targeting .NET 10
- `GET /` returns `Hello World!`
- `GET /api/products` returns three sample products as JSON
- `GET /api/products/{id}` returns a product, or HTTP 404 for an unknown ID
- ProductService, CartService, and ShopDbContext use scoped lifetimes through dependency injection
- EF Core with SQLite and migrations for Products, Carts, and CartItems
- `POST /api/carts` creates a persisted empty cart and returns HTTP 201 with its ID
- `GET /api/carts/{id}` returns the cart ID, or HTTP 404 for an unknown cart
- Cart responses use a DTO rather than exposing database entities
- A unique index prevents duplicate product rows within a cart
- Product endpoints read persisted data from SQLite through EF Core
- Three sample products are seeded in Development when the Products table is empty
- Product prices are stored as integer cent amounts
- Run locally and verified in the browser

## Getting started

Prerequisite: .NET 10 SDK.

```bash
cd B2BShop.Api
dotnet tool restore
dotnet ef database update
dotnet run --urls http://localhost:5080
```

Open http://localhost:5080 in your browser, or visit http://localhost:5080/api/products to view the sample products. Press Ctrl+C to stop the server.

The migration creates a local `shop.db` database with an empty `Products` table. On startup in Development, the application inserts three sample products if the table is empty. Existing products persist across restarts. Database files are excluded from Git; migrations and the local tool manifest are versioned.

## Next step

Add products to a cart with quantity validation and server-side pricing, then include cart items and totals in the response.
