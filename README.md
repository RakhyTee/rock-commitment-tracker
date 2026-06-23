# RockCommitmentTracker API

REST API for managing weekly commitments ("Rocks") for members of a professional accountability platform.


## Running Locally

**Prerequisites:** .NET 9 SDK

```bash
dotnet build
dotnet run --project RockCommitmentTracker.Api
```

API runs on `http://localhost:5107`.

All requests require an `X-Api-Key` header. Dev key is in `appsettings.json`:

```
X-Api-Key: dev-api-key
```

## Testing the Endpoints

Testing can be done via **Postman**, **Swagger UI** (`http://localhost:5107/swagger`), or **curl**.

### Create a Rock

```bash
curl -X POST http://localhost:5107/members/member-1/rocks \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: dev-api-key" \
  -d '{
    "title": "Close 3 new accounts",
    "category": "Revenue",
    "dueDate": "2026-06-30T00:00:00Z",
    "note": null
  }'
```

### Get All Rocks

```bash
curl http://localhost:5107/members/member-1/rocks \
  -H "X-Api-Key: dev-api-key"

# With status filter: Pending | Completed | Missed
curl "http://localhost:5107/members/member-1/rocks?status=Pending" \
  -H "X-Api-Key: dev-api-key"
```

### Update Rock Status

```bash
curl -X PATCH http://localhost:5107/members/member-1/rocks/{rockId} \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: dev-api-key" \
  -d '{ "newStatus": "Completed" }'
```

### Get Enriched Profile

```bash
curl http://localhost:5107/members/member-1/profile/enriched \
  -H "X-Api-Key: dev-api-key"
```


## Design Decisions

### Clean Architecture

Four projects, strict inward dependency rule:

```
Api → Application ← Infrastructure
            ↓
          Domain
```

- **Domain** — contains enums only (`RockCategory`, `RockStatus`). No logic, no dependencies.
- **Application** — contains the business logic, interfaces, validators, AutoMapper profiles. Owns the `Rock` model.
- **Infrastructure** — `IRockRepository` (in-memory `ConcurrentDictionary`), `IUserProfileClient` (JSONPlaceholder via typed `HttpClient`), Polly resilience pipeline (exponential backoff, 3 retries).
- **Api** — middleware, auth handler, endpoint mapping. Thin layer: request model → service call → response model via AutoMapper.

### Single `Middleware`

One middleware handles correlation ID stamping, request logging, and exception mapping to `ProblemDetails`. The correlation ID is pushed into Serilog's `LogContext` so every log line within that request carries it automatically.

### No MediatR

Four endpoints, one service — MediatR would be an overkill. The Api layer calls the service directly.

### Validation in the Application Layer

FluentValidation lives in `Application`, not controllers. Business rules are enforced regardless of how the service is called.


## Requirement 3 — Category Validation Strategy

Each `RockCategory` has its own validation rule:

| Category | Rule |
|---|---|
| Revenue | Due date must fall within the current quarter |
| Health | Title must be at least 10 characters |
| Career | `note` field is required |
| Other | No additional constraints |

Each rule is its own class implementing `IRockValidationStrategy`:

```csharp
public interface IRockValidationStrategy
{
    RockCategory Category { get; }
    string ErrorMessage { get; }
    bool IsValid(CreateRockCommand command);
}
```

`CreateRockValidator` receives all registered strategies via DI, builds a dictionary keyed by category, and dispatches through a single `Must()` rule:

```csharp
RuleFor(c => c)
    .Must(ExecuteStrategy)
    .WithMessage(c => GetStrategyErrorMessage(c.Category));
```

Adding a new category = one new class + one DI registration. No existing code changes, and each strategy is independently testable.

## Requirement 8 — Cloud Deployment (Azure)
 
**App Service**
 
Standard REST API so an App Service is the right fit — no cold starts, native Managed Identity, straightforward deployment.
 
**API Management**
 
API Management sits in front for rate limiting, versioning, and subscription key enforcement.
 
```
Client → API Management → App Service
```
 
**Secrets — Azure Key Vault**
 
No secrets in source control or pipeline variables. Everything sensitive goes through Key Vault, accessed via Managed Identity — no credentials needed at runtime.
 
App Service config references the secret directly:
 
```
Authentication__ApiKey = @Microsoft.KeyVault(SecretUri=https://<vault-name>.vault.azure.net/secrets/ApiKey/)
```
 
The existing `ApiKeyAuthenticationHandler` reads `configuration["Authentication:ApiKey"]` unchanged — only where the value comes from changes.
 
**Monitoring — Application Insights**
 
Application Insights is wired up for request telemetry, dependency tracking, and exception logging. Log stream is available directly in the Azure Portal under the App Service for real-time log tailing during debugging.
 
**Pipeline — Azure DevOps**
 
Code lives in GitHub. Azure DevOps is configured to point at the GitHub repo and runs the pipeline on push.
 
```yaml
stages:
  - Build     # restore, build, test, publish artifact
  - Sonar     # SonarCloud static analysis
  - Deploy    # AzureWebApp@1 → App Service
```

## What I'd Add Given More Time

- **Persistent storage** — swap `ConcurrentDictionary` for Azure Database for PostgreSQL with EF Core. Simple to plug in given the existing Clean Architecture setup — only Infrastructure changes.
- **Pagination** on `GET /members/{memberId}/rocks` — unbounded lists are a problem at scale.
- **Idempotency key** on `POST /rocks` — prevent duplicates from retried requests.
- **Integration tests** to cover the full HTTP stack (auth, middleware, problem details).
- **`IDateTimeProvider` abstraction** — `RevenueValidationStrategy` calls `DateTime.Today` directly, making it time-sensitive in tests. An injected clock fixes that.
