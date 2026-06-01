---
title: Running locally with Aspire
description: Start the whole stack — Postgres, Strapi, and the Blazor frontend — with the Aspire AppHost.
---

For local development, the **Aspire AppHost** orchestrates every dependency:
Postgres, the Strapi CMS (built from `Backend/backend-blog`), and the Blazor
frontend.

## Prerequisites

- **.NET 10 SDK**
- **Node.js 22** (Strapi runtime)
- A container runtime — **Docker** or **OrbStack**

## Start the AppHost

From the repository root:

```bash
dotnet run --project src/AspireStrapi.AppHost
```

This brings up:

1. **PostgreSQL** with a persistent data volume (`aspirestrapi-pgdata`).
2. **Strapi**, waiting for Postgres, configured with `DATABASE_CLIENT=postgres`
   and the required Strapi secrets (declared as Aspire parameters in
   `Program.cs`).
3. The **Blazor frontend** (`frontend-blog`), waiting for Strapi, with
   `Strapi__GraphQlEndpoint` pointed at Strapi's `/graphql`.

Open the **Aspire dashboard** (the URL is printed to the console) to view logs,
endpoints, and health for each resource.

## First-run seeding

On its first boot, Strapi seeds the database with **5 articles, 2 authors, and
5 categories** (with cover images) and grants the public role `find`/`findOne`
permissions — so the GraphQL API is immediately queryable without a token. See
[Strapi GraphQL setup](/AspireStrapi/strapi-graphql/) for details.

## Where to look

| Resource | Where |
| --- | --- |
| Blazor app | served by the `frontend-blog` resource (see the dashboard) |
| Strapi admin | `http://127.0.0.1:1337/admin` |
| Strapi GraphQL | `http://127.0.0.1:1337/graphql` |

When you're ready to ship a container deployment, continue to
[Deploying to OrbStack](/AspireStrapi/deploying/).
