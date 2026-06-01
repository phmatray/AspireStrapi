---
title: Overview
description: What AspireStrapi is, the technologies it uses, and how the pieces fit together.
---

AspireStrapi is a reference demo that connects a **Strapi 5 headless CMS** to a
**.NET 10 Aspire** application. Content is authored in the Strapi admin UI and
rendered **live** by a Blazor frontend that queries Strapi over **GraphQL**
using a typed **StrawberryShake** client. .NET Aspire orchestrates everything
for local development and publishes a Docker Compose deployment for OrbStack.

## Tech stack

- **.NET 10 Aspire** — app orchestration, service discovery, and Docker Compose publishing
- **Strapi 5.47** — headless CMS (`@strapi/plugin-graphql`), Postgres-backed in production
- **GraphQL + StrawberryShake 16** — net10-native code generation of a typed `BlogClient`
- **Blazor** — Server-Side Rendering (SSR) + Interactive Server presentation
- **PostgreSQL** — content database for Strapi
- **OrbStack** — local Docker host for the published Compose deployment

## Projects

The .NET solution lives under `src/` and is organised into hexagonal layers:

| Project | Role |
| --- | --- |
| `AspireStrapi.Domain` | Entities and value objects. No outward dependencies. |
| `AspireStrapi.Application` | Use cases (`ContentService`) and repository **ports** (interfaces). |
| `AspireStrapi.Infrastructure` | Strapi GraphQL **adapter** — the generated StrawberryShake `BlogClient` plus repository implementations. |
| `AspireStrapi.Web` | Blazor SSR + Interactive Server presentation. |
| `AspireStrapi.AppHost` | Aspire orchestration and Docker Compose publishing. |
| `AspireStrapi.ServiceDefaults` | Shared telemetry, health checks, and resilience defaults. |

`Backend/backend-blog` holds the Strapi 5 application.

## How content flows

1. An editor authors articles, authors, and categories in the **Strapi admin**.
2. The Blazor app calls a use case in **Application**, which depends only on a port.
3. **Infrastructure** implements that port by querying Strapi's **GraphQL** API
   with the generated `BlogClient`.
4. Blazor renders the result — no rebuild or redeploy is needed to publish content.

Continue to the [Architecture](/AspireStrapi/architecture/) page for the layer
diagram and dependency rules.
