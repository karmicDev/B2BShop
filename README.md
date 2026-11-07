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
- `POST /api/carts/{cartId}/items/{productId}/increment` increases the quantity by one, creating a missing item with quantity one
- `POST /api/carts/{cartId}/items/{productId}/decrement` decreases the quantity by one and removes the item when its quantity reaches zero
- Decrementing an absent item succeeds without changing the cart, provided the cart and product exist
- EF Core quantity concurrency checks and bounded retries handle concurrent updates and first-insert conflicts; exhausted retries return HTTP 409
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

Twelve integration test cases cover:

- Creating and retrieving an empty cart
- Repeated quantity-setting requests, a single cart item, and calculated totals
- Invalid quantities (-1, 0, and 100) without changing the cart
- Unknown carts and products returning HTTP 404
- Concurrent quantity-setting requests leaving one item with the requested quantity
- Ten concurrent increments producing one item with quantity ten
- Decrementing quantity three to two, removing quantity one, and decrementing an absent item

Tests use WebApplicationFactory in the Testing environment with a unique temporary SQLite file per factory and separate connections per context. Migrations create the test schema; the local shop.db database is not used. Test database files are removed when the factory is disposed.

## Next steps

1. Test concurrent decrements and increment/decrement quantity boundaries.
2. Add a dedicated endpoint for removing an entire cart item.
3. Add checkout with stored order prices and a Blazor frontend.
4. Later, add customer ownership/authentication, SQL Server support, and an Android client.

## Known limitations

Concurrency tests start overlapping HTTP requests but do not deterministically force every conflict path. Parallel decrement behavior and mixed increment/decrement operations still need dedicated tests.

Quantity changes retry recognized insert and optimistic concurrency conflicts up to 20 times. Under sustained contention, requests may return HTTP 409. Other database failures are not handled by this retry loop.

PUT sets an absolute quantity; increment and decrement apply relative changes. Automatically repeating an increment or decrement HTTP request after an uncertain network outcome can apply the action twice; request deduplication is not implemented.

Quantity is used as the concurrency token. It protects changes against a different current quantity, but does not detect an intervening change that returns to the same quantity. Concurrent deletion of carts or products is not specifically handled.

Cart ownership, authentication, a dedicated item-removal endpoint, checkout, and the web frontend are not implemented yet. SQLite-specific unique-constraint detection must be adapted when adding SQL Server support.
