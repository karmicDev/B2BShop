# B2BShop

A personal learning project: a B2B shop with an ASP.NET Core API, with a Blazor web frontend and an Android app planned for later.

## Current status

- ASP.NET Core targeting .NET 10
- `GET /` returns `Hello World!`
- Run locally and verified in the browser

## Getting started

Prerequisite: .NET 10 SDK.

```bash
cd B2BShop.Api
dotnet run --urls http://localhost:5080
```

Open http://localhost:5080 in your browser. Press Ctrl+C to stop the server.

## Next step

Add a `GET /api/products` endpoint with sample data.
