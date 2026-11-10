# B2BShop

A personal learning project evolving from a B2B shop into a restaurant ordering platform. The backend is built first with C#, ASP.NET Core, Entity Framework Core, and PostgreSQL. A SwiftUI iOS app is planned next, followed by a web frontend (potentially Blazor) and an Android app.

The repository currently contains the API and integration tests. Restaurant-specific models and client applications are planned, not implemented.

## Current status

- ASP.NET Core and EF Core targeting .NET 10
- PostgreSQL 17 through Npgsql, with Docker Compose for local development
- Scoped ProductService, CartService, and ShopDbContext
- EF Core migrations for Products, Carts, CartItems, and ASP.NET Core Identity
- Product prices stored as integer cent amounts; cart totals calculated using long integers
- Three sample products seeded in Development when the Products table is empty, using PostgreSQL-generated IDs
- ASP.NET Core Identity registration and login, with unique email addresses
- Cart DTOs expose calculated prices and totals rather than database entities
- A unique index prevents duplicate product rows within a cart
- Quantity concurrency checks and bounded retries handle competing updates and first-insert conflicts
- Seventeen integration test cases passing against isolated PostgreSQL test containers

## API endpoints

| Method | Path | Behavior |
| --- | --- | --- |
| GET | `/` | Returns `Hello World!` |
| GET | `/api/products` | Returns the persisted product catalog |
| GET | `/api/products/{id}` | Returns a product, or HTTP 404 |
| POST | `/api/auth/register` | Registers a user |
| POST | `/api/auth/login?useCookies=true` | Signs in using cookies |
| GET | `/api/auth/me` | Returns the authenticated user's ID and email; otherwise HTTP 401 |
| POST | `/api/carts` | Creates an empty cart; returns HTTP 201 with its ID and Location |
| GET | `/api/carts/{id}` | Returns items and calculated totals, or HTTP 404 |
| PUT | `/api/carts/{cartId}/items/{productId}` | Sets an absolute quantity from 1 to 99 |
| POST | `/api/carts/{cartId}/items/{productId}/increment` | Adds one, creating a missing item with quantity one |
| POST | `/api/carts/{cartId}/items/{productId}/decrement` | Subtracts one, removing the item at zero |
| DELETE | `/api/carts/{cartId}/items/{productId}` | Removes the entire item, regardless of quantity |

Identity also maps its built-in account-management endpoints under `/api/auth`; dedicated coverage currently focuses on registration, cookie login, and the current-user endpoint.

Cart endpoints are currently public and carts have no user ownership checks. Setting the same quantity again leaves the quantity unchanged. Decrementing or deleting an absent item succeeds without changing the cart, provided the cart and product exist. Invalid quantities return HTTP 400; unknown carts or products return HTTP 404. Exhausted concurrency retries return HTTP 409.

## Local development

Prerequisites:

- .NET 10 SDK
- Docker with Docker Compose and a running Linux container runtime
- On the current macOS setup, Colima provides the Docker runtime: `colima start`

Run the following commands from the repository root unless stated otherwise.

### Start PostgreSQL

Create a local `.env` file beside `compose.yaml`:

```dotenv
POSTGRES_PASSWORD=replace-with-your-local-development-password
```

The `.env` file is excluded from Git. Use a development-only password.

```bash
docker compose up -d
docker compose ps
```

Wait for the `postgres` service to report `healthy`. The container creates the `b2bshop` database and development user on first initialization. PostgreSQL is exposed only on `127.0.0.1:5432`.

Data persists in the named `postgres_data` volume. `docker compose stop` stops the service; `docker compose up -d` starts it again. Removing the volume deletes its database contents. Changing `.env` does not change the password of a database already initialized in the volume.

### Configure the API connection

Docker Compose reads `.env`; ASP.NET Core does not automatically read it. Store the API connection string in local .NET User Secrets using the same password:

```bash
dotnet user-secrets set "ConnectionStrings:ShopDatabase" 'Host=localhost;Port=5432;Database=b2bshop;Username=b2bshop;Password=REPLACE_WITH_YOUR_PASSWORD' --project B2BShop.Api
```

The API project already contains a UserSecretsId. User Secrets are stored outside the repository and loaded in Development. They are not encrypted and are intended for local development only. Do not commit passwords or connection strings containing credentials.

### Apply migrations and run

The local EF tool manifest is inside `B2BShop.Api`, so run these commands from that directory:

```bash
cd B2BShop.Api
dotnet tool restore
dotnet ef database update -- --environment Development
dotnet run --environment Development --urls http://localhost:5080
```

Open [the product catalog](http://localhost:5080/api/products). Startup in Development seeds three sample products if the table is empty. Existing products persist across restarts. Press Ctrl+C to stop the API.

Active PostgreSQL migrations are in `B2BShop.Api/Migrations`. Earlier SQLite migrations are archived in `docs/sqlite-migrations` and are not compiled into the API. `InitialPostgres` creates a fresh schema; it does not transfer data from an existing SQLite database. An old local `shop.db` is no longer used by the API when configured as above. The legacy SQLite connection string and package have been removed from the active application.

`RepairProductIdentitySequence` restarts the product identity sequence at 4 to accommodate the original development products with explicit IDs 1–3. It preserves existing rows and does not rewind the sequence on rollback. This migration targets that known initial dataset; it is not a general repair for databases containing arbitrary product IDs. New sample products omit explicit IDs and use PostgreSQL identity generation. IDs are identifiers, not a gapless counter or a product count.

## Tests

With Docker running, execute from the repository root:

```bash
dotnet test B2BShop.Api.Tests
```

To run a single test:

```bash
dotnet test B2BShop.Api.Tests --filter "FullyQualifiedName~CreateCart_CanBeRetrievedAsEmptyCart"
```

Tests use WebApplicationFactory in the Testing environment and Testcontainers.PostgreSql with `postgres:17`. Each factory owns a separate container with its own database and dynamically assigned host port. PostgreSQL must be started before creating the HTTP client. The factory replaces the application's database registration, applies the actual migrations, and inserts only the data requested by the test. Asynchronous disposal shuts down the test API and removes the database container.

Tests do not use the Docker Compose development database, its password, or its data. Docker must be available, but the Compose PostgreSQL service does not need to be running for tests. Initial runs may download additional container images. A container per test case prioritizes isolation over speed; the most recent reported run passed all 17 cases in approximately 60 seconds.

Coverage includes:

- Running the actual product initializer and then saving an additional product with a generated ID, verifying the count increases by one and the stored values match
- Creating and retrieving an empty cart
- Repeated quantity-setting requests, item prices, and calculated totals
- Invalid quantities (-1, 0, and 100) without changing the cart
- Unknown carts and products returning HTTP 404
- Concurrent quantity-setting requests without duplicate items
- Ten concurrent increments producing quantity ten
- Decrementing quantity three to two, removing quantity one, and decrementing an absent item
- Ten concurrent decrements reducing quantity eleven to one
- Removing an entire item while preserving another product, including repeated deletion
- Unauthenticated current-user requests returning HTTP 401
- Registration, cookie login, and retrieval of the authenticated user

## Planned scope

The next phase focuses on a realistic backend before starting client applications:

1. Model providers (`Place`: restaurant, bar, or kiosk) with addresses and coordinates, categories, and products with descriptions, image URLs, item information, availability, and dietary flags.
2. Add product option groups and choices with selection limits and price adjustments.
3. Extend the existing Identity user where needed; add provider-management permissions and protect cart access. A cart and order will each belong to one provider.
4. Implement ordering: validate availability and options, calculate authoritative prices on the server, and store order items with names, prices, and selected options as they were at purchase time. No real payment integration is planned initially.
5. Add and test the required customer and management endpoints incrementally.
6. Build the SwiftUI iOS client, then web and Android clients against the same API.

Loyalty points, digital stamp cards, and discount vouchers are secondary to the initial ordering flow. EUDI work is a separate project.

## Known limitations

Cart ownership and access protection, restaurant-specific models, orders, rewards, and client applications are not implemented yet. Identity role support is registered, but provider-management authorization and safe administrator assignment still need implementation. Logout coverage and a cookie/CSRF strategy are needed before browser-based writes.

Cart tests still use explicit product IDs for controlled fixtures. The product initialization test verifies the current seeding-and-insertion flow after all migrations; it does not separately reproduce the historical sequence repair against a pre-repair database.

Concurrency tests start overlapping HTTP requests but do not deterministically force every conflict path. Mixed increment/decrement operations and additional quantity-boundary cases still need dedicated tests.

Quantity changes retry optimistic concurrency conflicts and PostgreSQL unique violations for the cart/product index up to 20 times. Under sustained contention, requests may return HTTP 409. Other database failures are not handled by this retry loop.

PUT sets an absolute quantity; increment and decrement apply relative changes. Repeating a relative request after an uncertain network outcome can apply the action twice; request deduplication is not implemented.

Quantity is the concurrency token. It detects a different current quantity, but not an intervening change that returns to the same value. Concurrent deletion of carts or products is not specifically handled.
