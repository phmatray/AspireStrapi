# AspireStrapi "Next Level" Workflow — Design

**Date:** 2026-06-01
**Author:** Philippe Matray (with Claude Code)
**Status:** Approved (design), pending implementation

## Goal

Bring the AspireStrapi demo architecture "to the next level" via a single reusable,
mostly-autonomous multi-agent workflow saved at `.claude/workflows/next-level.js`.

## Confirmed constraints

| Decision | Choice |
|----------|--------|
| Autonomy | **Full auto end-to-end** (merge PRs, migrate, deploy, push docs) |
| .NET target | **net10.0** (current LTS; SDK 10.0.203 installed) |
| Architecture | **Full hexagonal / ports & adapters** (separate projects) |
| Deployment | **`aspire publish` + Docker Compose compute integration** on OrbStack |
| Deploy/verify | Workflow attempts; **main loop supervises** (ports, env, browser MCP) |
| Docs site | **Astro Starlight** on GitHub Pages |

## Starting state (observed 2026-06-01)

- Solution: `AspireStrapi.AppHost` + `AspireStrapi.ServiceDefaults` (net8.0, Aspire.Hosting 13.2.2),
  `AspireStrapi.BlazorBlog` (net8.0, StrawberryShake.Blazor 15.1.14).
- Backend: Strapi `5.42.1` with `@strapi/plugin-graphql ^5.0.0` already declared.
- Open PRs: #49 (nuget minor/patch), #50 (npm minor/patch) — both Renovate.
- Local tooling: .NET SDK 10.0.203, Node 22.16, Docker 29.4, OrbStack 2.1.3.

## Key technical risks

1. **Strapi 5 flattened GraphQL** — v5 removed the `data`/`attributes` wrapping
   (breaking change). Existing `.graphql` operations and README examples are v4-style
   and must be rewritten when the StrawberryShake client is regenerated.
2. **Hexagonal split of a Blazor project** — must keep dependency direction
   Domain ← Application ← Infrastructure / Web; no cycles.
3. **Live deploy + browser verification** — least reliable in blind subagents;
   supervised by the main loop.

## Phased pipeline

Mostly sequential (each phase gates on the previous building cleanly). Everything on
branch `feature/next-level`; every phase commits so steps are reversible.

| # | Phase | Work | Gate |
|---|-------|------|------|
| 0 | Baseline | branch `feature/next-level`; baseline `dotnet build` + Strapi `npm i` | build passes |
| 1 | Merge PRs | merge Renovate #49 + #50 into `dev`; rebase branch | merges clean |
| 2 | Migrate | net8→net10 (3 projects); bump Aspire + StrawberryShake + all NuGet; `npm update` Strapi | build + `npm i` pass |
| 3 | Hexagonal | split BlazorBlog → `Domain`, `Application`, `Infrastructure`, `Web`; reorganize `.sln` folders | build passes, refs acyclic |
| 4 | GraphQL | configure Strapi GraphQL; Public role read on Article/Category/Author; rewrite ops for flattened v5; regenerate client | client generates |
| 5 | Blazor demo | real pages: article list, detail (rich text + cover image), categories, authors; headless-CMS showcase page | build passes |
| 6 | Deploy | `aspire add docker`; `AddDockerComposeEnvironment` + `PublishAsDockerComposeService`; `aspire publish`; `docker compose up -d` on OrbStack | containers healthy |
| 7 | Verify | chrome-devtools MCP: navigate running app, assert articles render, check console/network, screenshot | renders, no console errors |
| 8 | Docs | rewrite README; scaffold Astro Starlight docs + GitHub Pages deploy workflow | docs build |

## Orchestration shape

- Phases 0–6 strictly sequential (hard dependencies).
- Fan-out within phases where safe: phase 3 parallel-reads existing code before splitting;
  phase 8 writes README + docs sections in parallel.
- Each phase returns structured status `{ ok, summary, commitSha, notes }`.
- On a failed gate, the workflow logs and **stops** rather than corrupting later phases.
- Phases 6–7 are attempted by the workflow but the main loop stays attached to recover
  port/env issues and re-drive the browser MCP directly.

## Deliverable

`.claude/workflows/next-level.js` — a named, re-runnable workflow. Resumable via
`resumeFromRunId` so a failed late phase doesn't redo earlier ones.
