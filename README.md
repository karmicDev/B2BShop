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
- `GET /api/carts/{id}` returns cart items and calculated totals, or HTTP 404 for an unknown cart
- `PUT /api/carts/{cartId}/items/{productId}` sets a product quantity between 1 and 99
- Setting the same quantity again leaves the quantity unchanged
- Prices are read from the product catalog; clients submit only quantities
- Invalid quantities return HTTP 400; unknown carts or products return HTTP 404
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

## Tests

The solution includes the API and an xUnit test project. From the repository root, run:

```bash
dotnet test B2BShop.Api.Tests
```

Seven integration test cases cover:

- Creating and retrieving an empty cart
- Repeated quantity-setting requests, a single cart item, and calculated totals
- Invalid quantities (-1, 0, and 100) without changing the cart
- Unknown carts and products returning HTTP 404

Tests use WebApplicationFactory in the Testing environment with a unique temporary SQLite file per factory and separate connections per context. Migrations create the test schema; the local shop.db database is not used. Test database files are removed when the factory is disposed.

## Next step

1. Restore the concurrent first-insert regression test and fix duplicate-insert conflicts using EF Core operations.
2. Add an atomic increment operation: ten successful increment requests must produce one cart item with quantity ten.
3. Add item removal, checkout with stored order prices, and a Blazor frontend.
4. Later, add customer ownership/authentication, SQL Server support, and an Android client.

## Known limitations

Concurrent first inserts of the same product into a cart can cause a UNIQUE constraint error. This was reproduced during testing and is not fixed yet; the concurrent regression test is currently absent from the test suite.

PUT sets an absolute quantity; it does not increment it. A plus-button increment operation is not implemented yet. Concurrent absolute quantity updates currently use last-write-wins behavior.

Cart ownership, authentication, item removal, checkout, and the web frontend are not implemented yet.
