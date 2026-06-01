---
title: Strapi GraphQL setup
description: How the Strapi 5 GraphQL plugin, public permissions, flattened v5 schema, and seed data are configured.
---

The backend is a **Strapi 5.47** application in `Backend/backend-blog`. It
exposes content through the GraphQL plugin and is seeded on first run.

## The GraphQL plugin

Strapi serves GraphQL via **`@strapi/plugin-graphql`** (version `5.47.0`,
matching the Strapi core). Once enabled, the API is available at:

- **GraphQL endpoint:** `http://127.0.0.1:1337/graphql`
- **Strapi admin:** `http://127.0.0.1:1337/admin`

Use `127.0.0.1` rather than `localhost` for consistency with the rest of the
deployment.

## Public permissions

The GraphQL queries run **without an auth token**, so the public role must be
granted read access to the content types. The seed script (`src/bootstrap.js`)
does this on first run by calling `setPublicPermissions` with `find` and
`findOne` for each type:

```js
await setPublicPermissions({
  article: ["find", "findOne"],
  category: ["find", "findOne"],
  author: ["find", "findOne"],
  global: ["find", "findOne"],
  about: ["find", "findOne"],
});
```

If you spin up a fresh Strapi instance manually, grant the same permissions
under **Settings → Users & Permissions → Roles → Public**.

## Flattened Strapi 5 schema

Strapi 5 returns a **flattened** GraphQL schema. The v4-style `data` /
`attributes` wrapping is **gone** — fields sit directly on the type, and each
entry carries a `documentId`. For example:

```graphql
query GetArticles {
  articles(sort: ["publishedAt:desc"]) {
    documentId
    title
    description
    slug
    publishedAt
    cover { url }
    category { name slug }
    author { name email avatar { url } }
  }
}
```

Note there is no `articles.data[].attributes.title` — just `articles[].title`.
Filtering and sorting use the v5 operator syntax, e.g.
`filters: { category: { slug: { eq: $slug } } }`.

GraphQL request bodies must use **lowercase JSON keys** (`query`, `variables`),
which matters when issuing raw requests (see the
[StrawberryShake client](/AspireStrapi/strawberryshake/) page for the
hand-written body query).

## Seed data

On its **first run**, `src/bootstrap.js` reads `data/data.json` and seeds:

- **5 articles** (each with a cover image and a dynamic-zone body)
- **2 authors** (with avatars)
- **5 categories**

It also uploads the cover/avatar images and sets the public permissions above.
The seed is idempotent — it records a `initHasRun` flag in the Strapi store, so
subsequent boots skip seeding.
