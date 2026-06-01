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

// ---- Phase 0: Baseline ------------------------------------------------------
results.baseline = await runPhase('Baseline', `
Establish a known-good baseline.
1. From branch dev, create and checkout ${BRANCH} (or checkout if it exists).
2. Run \`dotnet build AspireStrapi.sln\` and capture the result.
3. Run \`npm install\` in Backend/backend-blog and capture the result.
Gate: dotnet build succeeds AND npm install completes. Do NOT change code — this only verifies the starting point.
Report installed versions of dotnet SDK, node, docker, and orbstack in notes.`)

// ---- Phase 1: Merge Renovate PRs -------------------------------------------
results.merge = await runPhase('Merge PRs', `
Merge the two open Renovate dependency PRs.
1. \`gh pr list --state open\` to confirm #49 (nuget) and #50 (npm) (numbers may differ — match by branch renovate/nuget-minorpatch-updates and renovate/npm-minorpatch-updates).
2. Merge each into dev with \`gh pr merge <n> --squash --delete-branch\` (this phase IS allowed to update dev).
3. \`git checkout ${BRANCH}\` then \`git rebase dev\` (or merge dev in) to pull the updates onto our branch. Resolve any conflicts conservatively, preferring the updated dependency versions.
4. Run \`dotnet build AspireStrapi.sln\` and \`npm install\` in Backend/backend-blog.
Gate: both PRs merged AND branch builds. If a PR is already merged/closed, note it and continue.`)

// ---- Phase 2: Migrate to .NET 10 + latest deps -----------------------------
results.migrate = await runPhase('Migrate', `
Migrate the whole solution to .NET 10 and the latest dependencies.
1. In every .csproj set <TargetFramework>net10.0</TargetFramework> (AppHost, ServiceDefaults, BlazorBlog).
2. Bump Aspire.Hosting and any Aspire.* packages to the latest stable; bump StrawberryShake.* to latest stable; bump all other NuGet PackageReferences to latest stable compatible with net10. Use \`dotnet list package --outdated\` to discover, then update versions in the .csproj files (or via \`dotnet add package\`).
3. In Backend/backend-blog run \`npm update\` then \`npm install\`; ensure @strapi/plugin-graphql is present and on the 5.x line matching @strapi/strapi.
4. Address any net10 build breaks (analyzer/API changes) minimally.
Gate: \`dotnet build AspireStrapi.sln\` succeeds on net10 AND \`npm install\` clean. Commit.
Notes: list any packages you could NOT bump and why.`)

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
1. Open the Blazor app URL from the deploy phase notes.
2. Assert the Articles list renders at least one article (snapshot the DOM).
3. Navigate to an article detail page and assert the body + cover render.
4. Check console messages for errors and network requests for failed (4xx/5xx) GraphQL calls.
5. Take a screenshot of the articles list and the detail page; save paths under docs/.
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
