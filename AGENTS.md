# Etherna SSO

## Build, run, test

Target framework is **.NET 10** with `TreatWarningsAsErrors=true` and `AnalysisMode=AllEnabledByDefault` — warnings break the build.

```bash
dotnet restore EthernaSSO.sln
dotnet build EthernaSSO.sln -c Release
dotnet test  EthernaSSO.sln -c Release          # runs all xUnit test projects
dotnet test test/EthernaSSO.Domain.Tests/EthernaSSO.Domain.Tests.csproj    # single project
dotnet test --filter "FullyQualifiedName~UserBaseTest"                     # single class
dotnet test --filter "FullyQualifiedName~UserBaseTest.AddClaim_WithCustomClaim_AddsClaimAndReturnsTrue"  # single test
dotnet run  --project src/EthernaSSO                # local dev server
```

Frontend assets are bundled by Laravel Mix (webpack). The `EthernaSSO.csproj` MSBuild targets auto-run `npm install` + `npm run build-production` on `Debug` (if missing) and always on `Release`, so a plain `dotnet build` produces a working wwwroot. To iterate on JS/SCSS directly: `cd src/EthernaSSO && npm run watch`.

Docker: `docker build .` (uses `Dockerfile`, which also runs `dotnet test` as part of the build stage).

## Architecture

Four-project layered solution, plus two test projects:

- **`src/EthernaSSO.Domain`** — Pure domain layer. Aggregates live under `Models/` with the `<Name>Agg/` folder convention (e.g. `UserAgg`, `ClientAppAgg`). Base classes: `ModelBase`, `EntityModelBase<TKey>`. Domain events under `Events/` are dispatched via `Etherna.DomainEvents`. Exposes only `ISsoDbContext` / `ISharedDbContext` interfaces — no MongoDB types leak here.
- **`src/EthernaSSO.Persistence`** — MongODM implementations. `SsoDbContext` (main) and `SharedDbContext` (shared with other Etherna services) plus `ModelMaps/` (Sso, Shared) defining how domain entities serialize. Repositories under `Repositories/` (generic `DomainRepository`).
- **`src/EthernaSSO.Services`** — Application services and side effects. `Domain/` holds services that orchestrate domain operations (`UserService`, `Web3AuthnService`, `EmailSender`, `RazorViewRenderer`). `EventHandlers/` follows the `On<Event>Then<Action>Handler` convention and is auto-discovered by reflection in `ServiceCollectionExtensions.AddDomainServices` — adding a handler in this namespace registers it automatically. `Tasks/` contains Hangfire recurring jobs (scheduled in `Program.ConfigureApplication`).
- **`src/EthernaSSO`** — ASP.NET Core Razor Pages host. `Program.cs` wires everything: ASP.NET Identity (`UserBase`/`Role` with custom `UserStore`/`RoleStore`/`CustomUserManager`/`CustomUserValidator`), Duende IdentityServer (with composite `ClientAppStore` = in-memory `IdServerConfig.Clients` + DB-backed `ClientApp` entities), Hangfire (Mongo storage), Serilog → Elasticsearch, Prometheus `/metrics`, Scalar API reference at `/scalar/sso03`. Areas: `Identity` (account UI), `Admin` (admin pages, gated by `RequireAdministratorRolePolicy`), `Api` (REST endpoints under `/api`), `AlphaPass`.

Key cross-cutting points:

- **User model is polymorphic**: `UserBase` (abstract) → `UserWeb2` (email/password) or `UserWeb3` (Ethereum address). ASP.NET Identity is parameterized on `UserBase`.
- **Authentication uses a policy scheme** (`CommonConsts.UserAuthenticationPolicyScheme`): requests with `Authorization: Bearer …` go to JWT bearer; otherwise fall back to the Identity cookie. Service-to-service calls use a separate JWT scheme (`CommonConsts.ServiceAuthenticationScheme`). Custom requirements `DenyBannedAuthorizationRequirement` and `RequireRoleAuthorizationRequirement` are added to the default policy.
- **IdentityServer stores are split**: persisted grants, pushed authorization requests, server-side sessions, signing keys, and data-protection keys live in a separate `DataProtectionDb` (see `Program.cs`). The main `SSOServerDb` holds users/clients/domain data; `ServiceSharedDb` is shared with sibling Etherna services.
- **Hangfire queues** are declared in `Services/Tasks/Queues.cs` and pinned in `Program.AddHangfireServer` (`DB_MAINTENANCE`, `DOMAIN_MAINTENANCE`, `STATS`, `default`). The Hangfire server is **not started in Staging** (see condition in `ConfigureServices`).
- **MongODM change tracking**: every domain method that mutates a property *must* be annotated with `[PropertyAlterer(nameof(Prop))]` for each modified property — this is required, not optional. See the example under "Domain Entity Classes" below.

## Issue tracker

Bugs and features are tracked in Jira project **ESSO** (https://etherna.atlassian.net/projects/ESSO). Branch names follow `feature/ESSO-<id>-<slug>` / `improve/ESSO-<id>-<slug>` / `fix/ESSO-<id>-<slug>` — match this when creating branches.

# Coding Style

## General Principles

- Keep commits clean: only include changes strictly necessary for the task at hand.
- Exceptions to these conventions are accepted when strictly necessary or when they significantly improve code quality. Justify with a comment where needed.
- All elements (usings, properties, methods, fields, enum members, etc.) are always alphabetically ordered within their respective sections.
- Primary constructors are preferred everywhere the constructor is a simple parameter assignment — not limited to DI services.
- Keep code clean: remove unused variables, dead code, and redundant imports.

## Naming

- **Classes/Structs**: PascalCase (`UserService`, `ClientApp`)
- **Interfaces**: `I` prefix (`IUserService`, `ISsoDbContext`)
- **Async methods**: always `Async` suffix (`DeleteAsync`, `FindUserByAddressAsync`)
- **Properties**: PascalCase (`SharedInfoId`, `LastValidManifest`)
- **Private fields**: `_camelCase` only when backing a same-named property (`_items` for `Items`); otherwise plain `camelCase`
- **Primary constructor parameters**: `camelCase` without underscore
- **Constants**: PascalCase (`MaxEditHistory`, `SecretLength`)
- **Enums**: PascalCase type and members (`ClientAppType.WebApp`)
- **Namespaces**: `Etherna.SSOServer.<Layer>.<Feature>` (e.g. `Etherna.SSOServer.Domain.Models.UserAgg`)
- **DTOs**: `Dto` suffix (`UserDto`)
- **Event handlers**: `On<Event>Then<Action>Handler` (e.g. `OnUserLoginSuccessThenNotifyIdentityServerHandler`)
- **Custom exceptions**: `Exception` suffix, always `sealed`
- **Aggregate folders**: `<Name>Agg` suffix (`UserAgg`, `ClientAppAgg`)

## Code Organization

- One class per file, filename matches class name
- Namespace mirrors folder structure exactly
- Block-scoped namespaces: `namespace X { ... }` — NOT file-scoped
- Using directives inside namespace block, always alphabetically ordered and kept to the minimum necessary
- No global usings — each file declares its own imports

## Comments

Principal comments (generally multiline, important):
```csharp
// Capital start, ending period.
// Continued on next line if needed.
```

Secondary/separator comments:
```csharp
//no space, no capital, no ending period
```

## Member Ordering Within a Class

Use principal-style section comments to delimit groups:

```csharp
// Consts.
public const int MaxLength = 100;

// Fields.
private List<Item> _items = new();

// Constructors.
public MyEntity(string name) { ... }
protected MyEntity() { }

// Properties.
public virtual string Name { get; set; }

// Methods.
public virtual void DoSomething() { ... }

// Helpers.
private void InternalHelper() { ... }
```

## Class Design

- `internal sealed` for service implementations
- Primary constructors everywhere the constructor is a simple assignment

### Domain Entity Classes

- `public abstract` for base entity classes (`UserBase`, `ModelBase`, `EntityModelBase`)
- `virtual` on all properties for MongODM proxy support
- Use `public set` on properties by default; use `protected set` only when the property requires validation or invariant enforcement
- Protected parameterless constructor for ORM deserialization:
  ```csharp
  #pragma warning disable CS8618
  protected EntityName() { }
  #pragma warning restore CS8618
  ```
- Collection encapsulation with backing fields:
  ```csharp
  private List<string> _items = new();
  public virtual IEnumerable<string> Items
  {
      get => _items;
      protected set => _items = [..value ?? []];
  }
  ```
- `null!` for ORM-initialized properties: `public virtual string Name { get; protected set; } = null!;`
- Collection expressions for nullable collection setters: `[..value ?? []]`
- Equality: by ID for entities (in `EntityModelBase<TKey>`), by value for value objects
- `[PropertyAlterer(nameof(MyProp))]` on every method, for each property the method modifies. This is a MongODM limitation for change tracking:
  ```csharp
  [PropertyAlterer(nameof(LastValidManifest))]
  [PropertyAlterer(nameof(VideoManifests))]
  public virtual void AddManifest(VideoManifest manifest) { ... }
  ```

## Async Patterns

- Always suffix with `Async`
- `CancellationToken? cancellationToken = null` as optional last parameter
- Return `Task` or `Task<T>`, never `async void`
- No `ConfigureAwait(false)` — not needed in ASP.NET Core apps

## Null Handling

- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- `ArgumentNullException.ThrowIfNull(param)` for parameter validation
- `is null` / `is not null` (not `== null`)
- Prefer `null` over `default` as default value for optional parameters
- `??` and `??=` operators

## Formatting

- Allman braces (opening brace on new line)
- 4-space indentation
- Expression-bodied members for single expressions:
  ```csharp
  public bool IsExpired => Expiration.HasValue && DateTime.UtcNow > Expiration.Value;
  ```
- LINQ method chains: one operation per line, aligned
- Blank line between member sections

## C# Language Features

- Pattern matching: `is`, `is not`, type patterns, property patterns
- Switch expressions for multi-branch returns
- Primary constructors everywhere applicable
- Collection expressions: `[]`, `[..spread]`
- Target-typed `new()` when type is clear from context
- Tuple deconstruction for multiple return values

## LINQ

- Method syntax preferred over query syntax
- Query syntax only for complex join/groupby with multiple `from` clauses
- Fluent chaining, one operation per line for readability

## Dependency Injection

- Constructor injection exclusively
- Reflection-based event handler discovery
- `AddScoped` for domain services and DbContexts
- `AddTransient` for background tasks
- `AddSingleton` for configuration objects

## Testing (xUnit + Moq)

- `[Fact]` for basic tests, `[Theory]` for parameterized
- AAA pattern with section comments: `// Arrange.`, `// Action.`, `// Assert.`
- XUnit assertions: `Assert.Equal()`, `Assert.NotNull()`, `Assert.Throws<T>()`
- Moq for mocking: `new Mock<IService>()`
- Test projects mirror the project under test (`EthernaSSO.Domain.Tests`, `EthernaSSO.Persistence.Tests`)
- Domain tests use a builder helper pattern (e.g. `test/EthernaSSO.Domain.Tests/Helpers/UserWeb2Builder.cs`) to construct aggregates
