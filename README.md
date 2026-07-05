# Etherna SSO

[![License: AGPL-3.0](https://img.shields.io/badge/license-AGPL--3.0-blue)](COPYING)
[![Target framework](https://img.shields.io/badge/.NET-10-512BD4)](#building-and-testing)

**Etherna SSO** is the single sign-on and identity server for the [Etherna](https://etherna.io/) suite of
services. It authenticates users with both Web2 (email/password) and Web3 (Ethereum address) methods, and
acts as an OpenID Connect provider that issues tokens to the other Etherna applications.

Etherna SSO is an **application** — an ASP.NET Core server — not a library. This repository holds its full
source together with its build and deployment configuration.

## Contents

- [Features](#features)
- [Architecture](#architecture)
- [Running locally](#running-locally)
- [Configuration](#configuration)
- [Building and testing](#building-and-testing)
- [Docker](#docker)
- [Project layout](#project-layout)
- [Contributing](#contributing)
- [Issue reports](#issue-reports)
- [Questions? Problems?](#questions-problems)
- [License](#license)

## Features

- **Web2 login** — registration and sign-in with username/email and password, with email confirmation,
  password reset and account recovery.
- **Web3 login** — sign in by proving control of an Ethereum address via a challenge/response, and link an
  Ethereum address to an existing account.
- **Newsletter opt-in** — optional: during email verification the user can opt in (a verified email is then
  required), or later from the account email settings (a button shown only when not already subscribed). The
  verified contact is forwarded to an external newsletter service (e.g. Mailchimp) that owns the allowed list;
  SSO emails are never used for non-technical communications. Disabled by default.
- **Two-factor authentication** — TOTP authenticator apps, FIDO2/WebAuthn security keys (YubiKey, Touch ID,
  Windows Hello, …) and single-use recovery codes; at login a registered security key is preferred when no
  authenticator app is configured.
- **OpenID Connect provider** — built on Duende IdentityServer; issues identity/access tokens to the Etherna
  services, with in-memory and database-backed client registrations.
- **REST API** — endpoints under `/api` (including API-key authentication) with an interactive Scalar
  reference at `/scalar/sso03`.
- **Admin area** — user and client management for administrators.
- **Invitations & alpha pass** — optional invitation-gated registration and an alpha-pass request flow.
- **Observability** — structured logging to Elasticsearch through Serilog, and Prometheus metrics at
  `/metrics`.

## Architecture

A four-project layered solution (plus two test projects):

- **`EthernaSSO.Domain`** — pure domain layer: aggregates, entities and domain events; exposes only DbContext
  interfaces, with no persistence types leaking out.
- **`EthernaSSO.Persistence`** — MongoDB persistence via MongODM (model maps, repositories, the SSO and
  shared DbContexts).
- **`EthernaSSO.Services`** — application services, side effects, event handlers and Hangfire jobs.
- **`EthernaSSO`** — the ASP.NET Core Razor Pages host, wiring ASP.NET Identity, Duende IdentityServer,
  Hangfire, Serilog and the API.

The user model is polymorphic: `UserBase` → `UserWeb2` (email/password) or `UserWeb3` (Ethereum address).
Coding conventions and deeper architecture notes live in [AGENTS.md](AGENTS.md).

## Running locally

Prerequisites:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A reachable **MongoDB** replica set (as referenced by the connection strings in
  `appsettings.Development.json`)
- [Node.js](https://nodejs.org/) — front-end assets are bundled with npm during the build

Run the server:

```bash
dotnet run --project src/EthernaSSO
```

The build automatically runs `npm install` and bundles the front-end assets, so a plain `dotnet run`
produces a working site. To iterate on the front-end (SCSS/TypeScript) directly, run `npm run watch` inside
`src/EthernaSSO`. On the first run against an empty database, a first administrator account and its client
apps are seeded from the `DbSeed` settings.

## Configuration

Configuration uses the standard ASP.NET Core model. Values are resolved, in increasing order of precedence, from:

1. `appsettings.json` — committed defaults
2. `appsettings.{ASPNETCORE_ENVIRONMENT}.json` — e.g. `appsettings.Production.json`
3. **environment variables** — per-deployment values and secrets

Hierarchical keys use a separator: the colon form (`Section:SubSection:Key`) or the double-underscore form (`Section__SubSection__Key`), which is portable across all platforms. Array entries are indexed: `Section:Items:0`, `Section:Items:1`, …

Secrets (`*Password`, `*ServiceKey`, `*Secret`, `Duende:IdentityServer:LicenseKey`, `Encryption:EtherManagedPrivateKey`, MongoDB credentials) must be supplied via environment variables or a secret store at deploy time — never committed. Values marked **set in prod** below have no entry in `appsettings.Production.json` and must therefore come from the environment.

### Hosting (ASP.NET Core built-ins)

| Variable | Example | Notes |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | selects the `appsettings.{Environment}.json` overlay |
| `ASPNETCORE_URLS` | `http://+:80` | listening endpoints |

### Connection strings (MongoDB — all required)

| Key | Purpose |
|---|---|
| `ConnectionStrings:SSOServerDb` | main SSO database (users, clients, domain data) |
| `ConnectionStrings:ServiceSharedDb` | database shared with sibling Etherna services |
| `ConnectionStrings:DataProtectionDb` | ASP.NET data-protection + IdentityServer keys/grants/sessions |
| `ConnectionStrings:HangfireDb` | Hangfire job storage |

### Application

| Key | Default | Notes |
|---|---|---|
| `Application:CompactName` | `ethernaSso` | internal id, also the auth cookie name |
| `Application:DisplayName` | — | user-facing name (per environment) |
| `Application:EnableAlphaPassEmission` | `false` | enable alpha-pass requests |
| `Application:RequireInvitation` | `false` | require an invitation to register |

### Identity bootstrap (DbSeed)

| Key | Notes |
|---|---|
| `DbSeed:FirstAdminUsername` | first admin account username |
| `DbSeed:FirstAdminPassword` | **secret** — change in production |
| `DbSeed:Clients:<n>:…` | client apps created at db seeding, owned by the first admin (`ClientId`, `ClientName`, `ClientType`, `Secret`, `AllowedScopes`); used to initialize development environments with the same clients that production defines from the developer editor |

### Email

| Key | Default | Notes |
|---|---|---|
| `Email:CurrentService` | `FakeSender` | `Sendgrid` \| `Mailtrap` \| `FakeSender` |
| `Email:ServiceKey` | — | **secret** — provider API key |
| `Email:ServiceUser` | — | provider user (Mailtrap) |
| `Email:DisplayName` | `Etherna` | sender display name |
| `Email:SendingAddress` | `noreply@etherna.io` | sender address |

### Encryption

The Web2 managed-wallet private key (`UserWeb2.EtherManagedPrivateKey`) is stored encrypted at rest (AES-256-GCM). Existing managed keys become unreadable if this key is lost or changed.

| Key | Default | Notes |
|---|---|---|
| `Encryption:EtherManagedPrivateKey` | — (**set in prod**) | **secret** — base64-encoded 32-byte AES-256 key (`openssl rand -base64 32`); a dev default ships in `appsettings.Development.json` |

### Newsletter

Optional, opt-in only: a user can subscribe to the newsletter during email verification or later from the account email settings (see [Features](#features)). The verified contact is forwarded to the configured newsletter service, which owns the allowed list — emails stored in the SSO db are never used for non-technical communications. Disabled by default (`FakeService`, a no-op).

| Key | Default | Notes |
|---|---|---|
| `Newsletter:CurrentService` | `FakeService` | `Mailchimp` \| `FakeService` |
| `Newsletter:ApiKey` | — | **secret** — Mailchimp API key (includes the `-<dc>` data-center suffix) |
| `Newsletter:AudienceId` | — | Mailchimp audience (list) id |

### FIDO2 / WebAuthn (security-key 2FA)

| Key | Default | Notes |
|---|---|---|
| `Fido2:ServerName` | `Etherna SSO` | RP display name shown by the authenticator |
| `Fido2:ServerDomain` | — (**set in prod**) | RP ID — the origin host (no scheme/port), e.g. `sso.etherna.io`; must equal the origin host or a registrable parent of it |
| `Fido2:Origins:<n>` | — (**set in prod**) | allowed origin(s), e.g. `https://sso.etherna.io` |
| `Fido2:TimestampDriftTolerance` | `300000` | allowed clock drift (ms) |

### IdentityServer (Duende)

| Key | Notes |
|---|---|
| `Duende:IdentityServer:LicenseKey` | **secret** — Duende license, read automatically from configuration by IdentityServer v8 (may be empty in dev) |
| `IdServer:SsoServer:BaseUrl` | public SSO authority URL, e.g. `https://sso.etherna.io` |
| `IdServer:SsoServer:AllowUnsafeConnection` | dev only — allow an http authority |
| `IdServer:SsoServer:Clients:Webapp:Secret` | **secret** — SSO web-app client secret |
| `IdServer:Clients:<App>:…:Secret` | **secret** — downstream client secrets (Credit, Gateway, Index, …); client ids and base urls live in `appsettings*.json` |

### Logging & networking

| Key | Example | Notes |
|---|---|---|
| `Elastic:Urls:<n>` | `http://elasticsearch:9200` | Elasticsearch sinks for Serilog |
| `ForwardedHeaders:KnownNetworks:<n>` | `10.0.0.0/8` | trusted reverse-proxy networks |
| `Serilog:MinimumLevel:Default` | `Information` | global log level |
| `Serilog:MinimumLevel:Override:<Namespace>` | `Warning` | per-namespace override |

## Building and testing

```bash
dotnet restore EthernaSSO.sln
dotnet build   EthernaSSO.sln -c Release
dotnet test    EthernaSSO.sln -c Release      # runs the xUnit test projects
```

`TreatWarningsAsErrors=true` and `AnalysisMode=AllEnabledByDefault` are enabled across the solution, so
warnings break the build. A green `dotnet build EthernaSSO.sln` means everything compiled clean.

## Docker

The repository ships a `Dockerfile` that builds the server (and runs the test suite as part of the build
stage):

```bash
docker build -t etherna-sso .
```

Provide the runtime configuration (connection strings, FIDO2 origins, IdentityServer secrets, …) through
environment variables — see [Configuration](#configuration).

## Project layout

```
src/
  EthernaSSO.Domain        pure domain layer (aggregates, entities, domain events)
  EthernaSSO.Persistence   MongODM persistence (model maps, repositories, DbContexts)
  EthernaSSO.Services      application services, event handlers, Hangfire jobs
  EthernaSSO               ASP.NET Core host (Identity, IdentityServer, API, Razor Pages)
test/
  EthernaSSO.Domain.Tests        xUnit + Moq domain tests
  EthernaSSO.Persistence.Tests   xUnit persistence / serialization tests
```

## Contributing

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) and our
[Code of Conduct](CODE_OF_CONDUCT.md) before opening a pull request.

## Issue reports

If you've discovered a bug, or have an idea for a new feature, please report it to our issue manager based on
Jira: https://etherna.atlassian.net/projects/ESSO.

Detailed reports with stack traces, actual and expected behaviours are welcome.

## Questions? Problems?

For questions or problems please write an email to [info@etherna.io](mailto:info@etherna.io).

## License

![AGPL Logo](https://www.gnu.org/graphics/agplv3-with-text-162x68.png)

We use the GNU Affero General Public License v3 (AGPL-3.0) for this project.
If you require a custom license, you can contact us at [license@etherna.io](mailto:license@etherna.io).
