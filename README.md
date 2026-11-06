# B2BShop

A personal learning project: a B2B shop with an ASP.NET Core API, with a Blazor web frontend and an Android app planned for later.

## Current status

- ASP.NET Core targeting .NET 10
- `GET /` returns `Hello World!`
- `GET /api/products` returns three sample products as JSON
- Product prices are stored as integer cent amounts
- Run locally and verified in the browser

## Getting started

Prerequisite: .NET 10 SDK.

```bash
cd B2BShop.Api
dotnet run --urls http://localhost:5080
```

Open http://localhost:5080 in your browser, or visit http://localhost:5080/api/products to view the sample products. Press Ctrl+C to stop the server.

## Next step

Add a `GET /api/products/{id}` endpoint with a 404 response for unknown products.
