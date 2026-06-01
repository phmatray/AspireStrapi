export const meta = {
  name: 'next-level',
  description: 'Bring the AspireStrapi demo to the next level: merge PRs, migrate to .NET 10, apply hexagonal architecture, wire Strapi GraphQL + StrawberryShake, build a real Blazor demo, deploy on OrbStack, verify with browser MCP, and publish Astro Starlight docs to GitHub Pages.',
  whenToUse: 'Run once to modernize and showcase the AspireStrapi demo end-to-end. Resumable per phase.',
  phases: [
    { title: 'Baseline', detail: 'branch + known-good build' },
    { title: 'Merge PRs', detail: 'merge Renovate #49 + #50' },
    { title: 'Migrate', detail: 'net8 -> net10 + latest NuGet/npm' },
    { title: 'Hexagonal', detail: 'split into Domain/Application/Infrastructure/Web' },
    { title: 'GraphQL', detail: 'Strapi GraphQL + regenerate StrawberryShake (v5 flattened)' },
    { title: 'Blazor demo', detail: 'real Strapi-backed pages + headless CMS showcase' },
    { title: 'Deploy', detail: 'aspire publish + docker compose on OrbStack' },
    { title: 'Verify', detail: 'chrome-devtools MCP smoke test' },
    { title: 'Docs', detail: 'README + Astro Starlight on GitHub Pages' },
  ],
}

// ---- shared structured-output schema ----------------------------------------
const STATUS = {
  type: 'object',
  additionalProperties: false,
  required: ['ok', 'summary'],
  properties: {
    ok: { type: 'boolean', description: 'true only if the phase gate passed (build/install/merge succeeded)' },
    summary: { type: 'string', description: 'one-paragraph summary of what changed' },
    commitSha: { type: 'string', description: 'short sha of the commit this phase made, or empty' },
    gate: { type: 'string', description: 'the exact verification command run and its result' },
    notes: { type: 'string', description: 'anything the next phase or a human supervisor must know (warnings, manual follow-ups)' },
    artifacts: { type: 'array', items: { type: 'string' }, description: 'key files created or changed' },
  },
}

const BRANCH = 'feature/next-level'
const REPO_RULES = `
Working directory is the AspireStrapi repo root. Rules:
- Work ONLY on branch ${BRANCH} (create it from dev if missing, otherwise checkout).
- Make focused commits ending with the Co-Authored-By trailer for "Claude Opus 4.8 (1M context) <noreply@anthropic.com>".
- Do NOT push, open PRs, or merge to dev unless your phase explicitly says so.
- Run the stated gate command and report its real output. Set ok=false (do not fake success) if it fails.
- BUILD ENV: use StrawberryShake 16.x (its codegen ships net8/net9/net10 tool variants and runs natively on the installed net10 runtime — no roll-forward or extra runtime needed).
- Return the structured status object as your final result.`

// helper that runs one phase agent and halts the pipeline on a failed gate
async function runPhase(title, prompt) {
  phase(title)
  const r = await agent(`${prompt}\n${REPO_RULES}`, { label: title.toLowerCase().replace(/\s+/g, '-'), phase: title, schema: STATUS })
  log(`[${title}] ok=${r?.ok} ${r?.summary || ''}`)
  if (!r || !r.ok) {
    log(`[${title}] GATE FAILED — stopping pipeline. notes: ${r?.notes || 'n/a'}`)
    throw new Error(`Phase "${title}" failed its gate: ${r?.notes || r?.summary || 'unknown'}`)
  }
  return r
}

const results = {}

// ---- Phase 0: Baseline (record-only, NON-gating) ----------------------------
// The repo is intentionally allowed to be broken here — capturing the starting
// state (including pre-existing build/install failures) is the whole point. The
// Migrate phase fixes them. So this phase ALWAYS proceeds.
phase('Baseline')
results.baseline = await agent(`
Record the starting state of the repo. Do NOT change any code or commit.
1. From branch dev, create and checkout ${BRANCH} (or checkout if it exists).
2. Run \`dotnet build AspireStrapi.sln\` and capture all errors/warnings verbatim.
3. Run \`npm install\` in Backend/backend-blog and capture the result verbatim.
4. Report installed versions of dotnet SDK + runtimes, node, docker, orbstack.
This is a RECORD-ONLY phase: set ok=true regardless of build/install outcome, and put the
full list of every pre-existing build error and npm error into \`notes\` so later phases can fix them.
${REPO_RULES}`, { label: 'baseline', phase: 'Baseline', schema: STATUS })
log(`[Baseline] recorded. notes: ${results.baseline?.notes || 'n/a'}`)

// ---- Phase 1: Merge Renovate PRs (NO build gate — repo doesn't build until Migrate) --
results.merge = await runPhase('Merge PRs', `
Ensure the open Renovate dependency PRs are merged and our branch is on top of dev.
1. \`gh pr list --state all --limit 8\` — match the renovate PRs by branch renovate/nuget-minorpatch-updates and renovate/npm-minorpatch-updates.
2. For any that are still OPEN, merge into dev with \`gh pr merge <n> --squash --delete-branch\` (this phase IS allowed to update dev). If they are already MERGED, note that and skip.
3. \`git checkout ${BRANCH}\` then ensure dev is an ancestor (\`git merge --ff-only dev\` or \`git rebase dev\`). Resolve any conflicts preferring the updated dependency versions.
4. Confirm with \`git log --oneline -3 dev\` and \`git diff --name-only dev ${BRANCH}\`.
Gate: the renovate PRs are MERGED (or confirmed already merged) AND ${BRANCH} has dev as an ancestor. DO NOT run a build here — the repo intentionally does not build until the Migrate phase fixes it. Commit only if you actually changed tracked files.`)

// ---- Phase 2: Migrate to .NET 10 + latest deps -----------------------------
results.migrate = await runPhase('Migrate', `
Migrate the whole solution to .NET 10 and the latest dependencies, AND fix the pre-existing build/install failures recorded by the Baseline phase:
--- BASELINE NOTES (known issues to fix) ---
${results.baseline?.notes || 'n/a'}
--- END ---

These four known issues MUST be resolved:
(A) ServiceDefaults Extensions.cs: 'IHttpClientBuilder.UseServiceDiscovery' no longer exists in the newer Microsoft.Extensions.ServiceDiscovery — migrate to the current API (the AddServiceDiscovery on the service collection + the new http client wiring per the latest Aspire ServiceDefaults template). Regenerate ServiceDefaults from the current Aspire template shape if cleaner.
(B) AppHost uses the DEPRECATED Aspire workload (NETSDK1228). Migrate to the NuGet-based Aspire app model per https://aka.ms/aspire/update-to-sdk: add the Aspire.AppHost.Sdk to the AppHost csproj, reference latest Aspire.Hosting.* from NuGet, remove workload reliance.
(C) StrawberryShake build-time codegen on the old 15.x targeted net9 only and failed without a net9 runtime. Resolve by upgrading StrawberryShake.* (and the strawberryshake.tools entry in .config/dotnet-tools.json) to the latest stable 16.x, whose codegen ships a net10 tool variant and runs natively on net10 — no roll-forward or extra runtime needed.
(D) npm ERESOLVE: react@19 conflicts with @strapi/strapi 5.42.1 (peer wants react 17/18). Resolve by aligning versions — prefer bumping Strapi to the latest 5.x that supports React 19 if available; otherwise pin react/react-dom to ^18. Use --legacy-peer-deps only as a last resort and note it.

Then the general migration:
1. In every .csproj set <TargetFramework>net10.0</TargetFramework> (AppHost, ServiceDefaults, BlazorBlog).
2. Bump Aspire.* and StrawberryShake.* to latest stable; bump all other NuGet to latest stable compatible with net10 (\`dotnet list package --outdated\`). Also resolve the OpenTelemetry.Exporter.OpenTelemetryProtocol NU1902 vulnerability warning by bumping to a patched version.
3. In Backend/backend-blog get \`npm install\` clean, then \`npm update\`; ensure @strapi/plugin-graphql matches the @strapi/strapi 5.x version.
Gate: \`dotnet build AspireStrapi.sln\` succeeds on net10 (including StrawberryShake codegen) AND \`npm install\` completes without ERESOLVE. Commit.
Notes: list any packages you could NOT bump and why, and which option you used for issue (C) and (D).`)

// ---- Phase 3: Hexagonal architecture ---------------------------------------
phase('Hexagonal')
// parallel scout reads of the existing Blazor project before restructuring
const scout = await parallel([
  () => agent('Read AspireStrapi.BlazorBlog/* and list every page/component, the StrawberryShake usage, DI/Program.cs wiring, and the GraphQL client config. Return a concise inventory.', { label: 'scout-blazor', phase: 'Hexagonal' }),
  () => agent('Read AspireStrapi.sln and all .csproj files. Map current project references and propose the exact new solution layout for a hexagonal split (Domain, Application, Infrastructure, Web) with reference directions and where StrawberryShake belongs (Infrastructure adapter). Return the plan.', { label: 'scout-sln', phase: 'Hexagonal' }),
])
results.hexagonal = await runPhase('Hexagonal', `
Apply full hexagonal (ports & adapters) architecture to the BlazorBlog and reorganize the solution.

Use this inventory of the current code:
--- BLAZOR INVENTORY ---
${scout[0] || 'n/a'}
--- SOLUTION PLAN ---
${scout[1] || 'n/a'}
--- END ---

Create four projects under a new src/ layout (keep names AspireStrapi.* prefixed):
- AspireStrapi.Domain        (entities: Article, Category, Author, Tag; value objects; no external deps)
- AspireStrapi.Application   (ports: interfaces like IArticleRepository/IContentService; DTOs; use-cases; depends only on Domain)
- AspireStrapi.Infrastructure(adapters: the StrawberryShake GraphQL client + an implementation of the Application ports that maps GraphQL types -> Domain; depends on Application + Domain)
- AspireStrapi.Web           (the Blazor presentation; depends on Application + Infrastructure for DI only)
Move the AppHost + ServiceDefaults under src/ too. Reorganize AspireStrapi.sln with solution folders (src, Backend, docs). Keep reference direction acyclic: Web -> Application/Infrastructure, Infrastructure -> Application -> Domain.
Wire DI in Web/Program.cs so pages depend on Application ports, not on StrawberryShake directly.
Gate: \`dotnet build AspireStrapi.sln\` succeeds. Commit.`)

// ---- Phase 4: Strapi GraphQL + StrawberryShake (v5 flattened) ---------------
results.graphql = await runPhase('GraphQL', `
Enable and wire GraphQL between Strapi 5 and StrawberryShake.
STRAPI:
1. Ensure @strapi/plugin-graphql is installed and enabled in Backend/backend-blog/config/plugins.{ts,js} (create the file if missing) with playground enabled in dev.
2. Configure the Public role to allow find/findOne on Article, Category, Author (Strapi seeds these via config/bootstrap or document the manual step clearly in notes — prefer a bootstrap script in src/index.ts that grants public read on first run).
INTEGRATION (CRITICAL — Strapi 5 flattened GraphQL):
3. Strapi 5 removed the v4 data/attributes wrapping. Rewrite every .graphql operation (e.g. GetArticles) to the flattened v5 schema shape (e.g. articles { documentId title description cover { url } category { name } author { name } }).
4. In AspireStrapi.Infrastructure, point the StrawberryShake client at http://localhost:1337/graphql, refresh the schema (\`dotnet graphql update\` or the StrawberryShake tooling), and regenerate the client so generated types match the flattened schema.
5. Update the Infrastructure adapter to map the regenerated GraphQL types to Domain entities.
Gate: \`dotnet build AspireStrapi.sln\` succeeds and the generated client compiles against the new ops. Commit.
Notes: include the exact GraphQL endpoint, any schema-fetch command, and whether public permissions are automated or manual.`)

// ---- Phase 5: Real Blazor demo + headless CMS showcase ----------------------
results.demo = await runPhase('Blazor demo', `
Build a real demo on top of the hexagonal stack that showcases Strapi as a headless CMS.
Pages (Blazor, in AspireStrapi.Web, consuming Application ports only):
- Articles list: title, excerpt, cover image, category badge, author, published date.
- Article detail (/articles/{documentId or slug}): rich text body, cover image, author, category, tags.
- Categories page: list categories with article counts; filter articles by category.
- Authors page: list authors with their articles.
- A "Headless CMS" explainer page that states the content is authored in Strapi admin (http://localhost:1337/admin) and rendered live by Blazor via GraphQL — include a short note on how editing in Strapi reflects in the app.
Use clean, modern, accessible markup and the existing styling approach; loading + error states for every async fetch.
Gate: \`dotnet build AspireStrapi.sln\` succeeds. Commit.`)

// ---- Phase 6: Deploy on OrbStack via aspire publish (Docker compute) --------
results.deploy = await runPhase('Deploy', `
Deploy the app on OrbStack using the Aspire Docker Compose compute integration.
1. In the AppHost dir run \`aspire add docker\` (adds Aspire.Hosting.Docker).
2. In AppHost Program.cs: \`var compose = builder.AddDockerComposeEnvironment("compose");\` and add \`.PublishAsDockerComposeService((r, s) => { ... })\` to the Strapi, Blazor Web, and Postgres resources. Ensure Strapi has a Dockerfile (use the strapi dockerize approach if missing) and Postgres has a persistent volume.
3. Run \`aspire publish\` to generate docker-compose.yml + .env into a publish/ output dir.
4. Deploy on OrbStack: \`docker compose -f <generated>/docker-compose.yml up -d --build\`. OrbStack is the active Docker context.
5. Wait for containers to become healthy; \`docker compose ps\` and curl the Strapi /graphql and the Blazor app to confirm they respond.
Gate: containers are up and both the Strapi GraphQL endpoint and the Blazor app return HTTP 200. Commit the deploy config.
Notes: print the resolved URLs/ports for Strapi admin, GraphQL, and the Blazor app. If a step needs a one-time interactive token (e.g. Strapi admin), STOP and report it in notes for the supervisor.`)

// ---- Phase 7: Browser MCP verification --------------------------------------
results.verify = await runPhase('Verify', `
Smoke-test the running app with the chrome-devtools MCP tools (load their schemas via ToolSearch: "select:mcp__plugin_chrome-devtools-mcp_chrome-devtools__navigate_page,mcp__plugin_chrome-devtools-mcp_chrome-devtools__new_page,mcp__plugin_chrome-devtools-mcp_chrome-devtools__take_snapshot,mcp__plugin_chrome-devtools-mcp_chrome-devtools__take_screenshot,mcp__plugin_chrome-devtools-mcp_chrome-devtools__list_console_messages,mcp__plugin_chrome-devtools-mcp_chrome-devtools__list_network_requests").
KNOWN-GOOD ENDPOINTS (this deployment): Blazor app = http://127.0.0.1:8090, Strapi = http://127.0.0.1:1337. Use 127.0.0.1, NOT localhost (localhost resolves to IPv6 ::1 and the containers only answer on IPv4). Note host port 8090 (not 8080) is intentional to avoid colliding with other local services such as an OrbStack k8s load-balancer.
First confirm the stack is the AspireStrapi one (curl http://127.0.0.1:8090/ and check the page title is "Strapi headless CMS demo", not some other local app on the port). Seed content exists: 5 articles, 2 authors, 5 categories with cover images.
1. Open http://127.0.0.1:8090/articles.
2. Assert the Articles list renders all seeded articles (snapshot the DOM).
3. Navigate to an article detail page (/articles/{documentId}) and assert the body + cover render (the body comes from the dynamic-zone blocks query).
4. Check console messages for errors (cover/avatar images must load from http://127.0.0.1:1337/uploads/*, NOT file:// or strapi:1337) and network requests for failed (4xx/5xx) GraphQL calls (the body query must be 200, not 400).
5. Take a screenshot of the articles list and the detail page; save under docs/verification/.
Gate: articles render on list + detail AND there are no console errors and no failed GraphQL requests.
Notes: if the browser MCP is unavailable in this run, set ok=false with a clear note so the supervising main loop can drive it.`)

// ---- Phase 8: README + Astro Starlight docs on GitHub Pages -----------------
phase('Docs')
const docParts = await parallel([
  () => agent(`Rewrite the repository README.md to reflect the new architecture: .NET 10 Aspire, hexagonal (Domain/Application/Infrastructure/Web), Strapi 5 headless CMS with GraphQL, StrawberryShake client (flattened v5 schema), OrbStack Docker Compose deploy via \`aspire publish\`, and a link to the docs site. Include accurate quickstart + architecture diagram (mermaid). Use these phase summaries for accuracy:\n${JSON.stringify({migrate: results.migrate?.summary, hexagonal: results.hexagonal?.summary, graphql: results.graphql?.summary, deploy: results.deploy?.summary}, null, 2)}\nWrite the file and return what you wrote.`, { label: 'readme', phase: 'Docs' }),
  () => agent('Scaffold an Astro Starlight docs site under docs/site (npm create astro@latest with the starlight template, non-interactive). Author starter pages: Overview, Architecture (hexagonal ports & adapters), Strapi GraphQL setup, StrawberryShake client, Running locally with Aspire, Deploying to OrbStack. Configure astro.config for a GitHub Pages base path. Return the file layout.', { label: 'starlight', phase: 'Docs' }),
])
results.docs = await runPhase('Docs', `
Finalize documentation and GitHub Pages.
The README has been rewritten and an Astro Starlight site scaffolded under docs/site:
--- README ---
${docParts[0] || 'n/a'}
--- STARLIGHT ---
${docParts[1] || 'n/a'}
--- END ---
1. Verify the README renders and links resolve.
2. \`cd docs/site && npm install && npm run build\` to confirm the Starlight site builds.
3. Create .github/workflows/docs.yml that builds docs/site and deploys to GitHub Pages on push to dev (actions/configure-pages, upload-pages-artifact, deploy-pages; set the correct base path).
Gate: \`npm run build\` of the docs site succeeds. Commit everything.
Notes: state the GitHub Pages URL the workflow will publish to and whether Pages must be enabled in repo settings (manual one-time step).`)

log('next-level pipeline complete')
return {
  branch: BRANCH,
  phases: Object.fromEntries(Object.entries(results).map(([k, v]) => [k, { ok: v?.ok, summary: v?.summary, commit: v?.commitSha }])),
  supervisorFollowUps: Object.entries(results)
    .filter(([, v]) => v?.notes)
    .map(([k, v]) => `${k}: ${v.notes}`),
}
