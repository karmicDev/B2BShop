# B2BShop

A personal learning project: a B2B shop with an ASP.NET Core API, with a Blazor web frontend and an Android app planned for later.

## Current status

- ASP.NET Core targeting .NET 10
- `GET /` returns `Hello World!`
- `GET /api/products` returns three sample products as JSON
- `GET /api/products/{id}` returns a product, or HTTP 404 for an unknown ID
- ProductService is registered through dependency injection
- EF Core with SQLite, ShopDbContext, and an initial Products migration
- API responses still use in-memory sample data; database integration is the next step
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

The migration creates a local `shop.db` database with an empty `Products` table. Database files are excluded from Git; migrations and the local tool manifest are versioned.

## Next step

Connect ProductService to ShopDbContext and populate the database with sample products.
