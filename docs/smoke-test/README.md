# Smoke test — Blazor frontend (Docker Compose deploy)

Date: 2026-06-01
Branch: feature/next-level
Tooling: chrome-devtools MCP

## Target

Deploy phase (`publish/docker-compose.yaml`) exposes the Blazor frontend on host
port 8080. NOTE: `localhost` resolves to IPv6 `::1`, which hangs; the container
publishes on IPv4, so use `http://127.0.0.1:8080/`.

Running containers (OrbStack):

- `publish-frontend-blog-1` — image `aspirestrapi-frontend-blog:local`,
  entrypoint `dotnet /app/AspireStrapi.Web.dll`, `0.0.0.0:8080->8080`
- `publish-strapi-1` — `1337:1337`
- `publish-postgres-1`, `publish-compose-dashboard-1`

## Result: FAIL (gate not met)

The deployed frontend does NOT serve the AspireStrapi Strapi blog. It serves a
different/stale application ("HorizonHub", v1.7.13 — an airport ops dashboard).

Evidence:

- `GET http://127.0.0.1:8080/` → 200, title `HorizonHub`. Nav links are
  HorizonHub routes (`/flights`, `/vouchers`, `/ceia`, `/pmr` ...) that do not
  exist in this repo. See `home-horizonhub.png`.
- `GET http://127.0.0.1:8080/articles` → **HTTP 404** (chrome error page). See
  `articles-404.png`.
- Repo routes all 404 in the deployed app: `/articles`, `/articles/{DocumentId}`,
  `/headless-cms`, `/authors`, `/categories`.
- Baked static-asset manifest in the image is `HorizonHub.Web.*.styles.css`,
  confirming the published content is the wrong app despite the `AspireStrapi.Web.dll`
  assembly name and `aspirestrapi-frontend-blog:local` tag.

Console / network on the served pages were clean (Blazor WebSocket connected,
`_blazor/initializers` and `negotiate` returned 200, no console errors), but this
is irrelevant because the articles UI is absent.

## Checklist vs. gate

1. Open Blazor app URL — done (`http://127.0.0.1:8080/`).
2. Articles list renders >= 1 article — FAIL: `/articles` returns 404.
3. Article detail (body + cover) — NOT REACHABLE: no list, route 404.
4. Console errors / failed GraphQL — none observed on served pages, but N/A.
5. Screenshots saved — `home-horizonhub.png`, `articles-404.png`.

## Root cause / handoff for supervising loop

The `aspirestrapi-frontend-blog:local` image was published from the wrong source
(the HorizonHub app), not this repo's `dotnet/AspireStrapi.Web`. The image must be
rebuilt from the correct project before the smoke test can pass:

    DOTNET_ROLL_FORWARD=LatestMajor \
    dotnet publish dotnet/AspireStrapi.Web/AspireStrapi.Web.csproj \
      -t:PublishContainer -p:ContainerImageTag=local \
      -p:ContainerRepository=aspirestrapi-frontend-blog

then `docker compose up -d --build` in `dotnet/AspireStrapi.AppHost/publish` and
re-run this smoke test against `http://127.0.0.1:8080/articles`.
