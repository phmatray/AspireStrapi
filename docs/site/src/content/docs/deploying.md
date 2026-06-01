---
title: Deploying to OrbStack
description: Publish a Docker Compose deployment with aspire publish and run it on OrbStack.
---

AspireStrapi ships to a container host using the Aspire **Docker Compose compute
integration**. The AppHost declares a Compose environment
(`AddDockerComposeEnvironment("compose")`) and marks each resource with
`PublishAsDockerComposeService(...)`, so `aspire publish` can emit a complete
deployment.

## Publish the Compose files

```bash
aspire publish src/AspireStrapi.AppHost
```

This generates:

- `src/AspireStrapi.AppHost/publish/docker-compose.yaml`
- `src/AspireStrapi.AppHost/publish/.env`

The `.env` carries the Strapi secrets and Postgres credentials that were
declared as Aspire parameters in `Program.cs`.

## Deploy on OrbStack

With [OrbStack](https://orbstack.dev/) running as the Docker host:

```bash
docker compose -f src/AspireStrapi.AppHost/publish/docker-compose.yaml up -d
```

Compose builds the Strapi image from `Backend/backend-blog`, starts Postgres
with a persistent volume, and runs the Blazor frontend.

## URLs and ports

| Service | URL |
| --- | --- |
| Blazor app | `http://127.0.0.1:8090` |
| Strapi admin | `http://127.0.0.1:1337/admin` |
| Strapi GraphQL | `http://127.0.0.1:1337/graphql` |

Always use **`127.0.0.1`**, not `localhost`.

The frontend is published on host port **8090** (instead of the conventional
8080) to avoid colliding with other local services — such as an OrbStack
Kubernetes load balancer that commonly occupies 8080.

## The `Strapi__PublicBaseUrl` media note

Inside the Compose network, the frontend reaches Strapi at `strapi:1337`. But
the **browser** can't resolve that host — it needs a publicly reachable URL to
load media (cover images, avatars) from `/uploads/*`. So the AppHost injects:

```text
Strapi__PublicBaseUrl=http://127.0.0.1:1337
```

The Blazor app uses this base URL when rendering media links, so images load
from the host-published Strapi on port 1337 rather than the in-network
`strapi:1337` address. Without it, media URLs would point at an unreachable host
and images would break in the browser.
