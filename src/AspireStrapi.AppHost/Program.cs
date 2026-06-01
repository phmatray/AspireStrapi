using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Docker Compose compute environment.
// `aspire publish` uses this to emit a docker-compose.yml + .env that can be
// deployed on any Docker host (here: OrbStack).
// ---------------------------------------------------------------------------
var compose = builder.AddDockerComposeEnvironment("compose");

// ---------------------------------------------------------------------------
// Strapi secrets (the keys Strapi requires to boot in production). Declared as
// parameters with default values so they flow into the generated .env file.
// ---------------------------------------------------------------------------
var appKeys = builder.AddParameter("strapi-app-keys", "FO7XR1+fPZ2EY6Oc12fizA==,oB6b+m7JHXkrUfn3rKCe2Q==");
var apiTokenSalt = builder.AddParameter("strapi-api-token-salt", "X2i3Po24w8EDsBj7QHktaw==");
var adminJwtSecret = builder.AddParameter("strapi-admin-jwt-secret", "yt+WWFQLKJWVA0mGh9brCw==", secret: true);
var transferTokenSalt = builder.AddParameter("strapi-transfer-token-salt", "mLCJ555V4lmm5SD8AXQeeA==");
var jwtSecret = builder.AddParameter("strapi-jwt-secret", "8Lfnqf7tYZ8zx1z2r21CBQ==", secret: true);
var encryptionKey = builder.AddParameter("strapi-encryption-key", "ovfPuQPxgP9ZMx5YxURZ6g==", secret: true);

// Database credentials – declared explicitly so both Postgres and Strapi agree.
var dbUser = builder.AddParameter("postgres-username", "strapi");
var dbPassword = builder.AddParameter("postgres-password", "strapi", secret: true);

// ---------------------------------------------------------------------------
// PostgreSQL with a persistent data volume.
// ---------------------------------------------------------------------------
var postgres = builder
    .AddPostgres("postgres", userName: dbUser, password: dbPassword)
    .WithDataVolume("aspirestrapi-pgdata")
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Name = "postgres";
        service.Restart = "unless-stopped";
    });

var strapiDb = postgres.AddDatabase("strapidb", databaseName: "strapi");

// ---------------------------------------------------------------------------
// Strapi CMS – built from the Backend/backend-blog Dockerfile, backed by
// Postgres, exposed on port 1337.
// ---------------------------------------------------------------------------
var strapi = builder
    .AddDockerfile("strapi", "../../Backend/backend-blog")
    .WithHttpEndpoint(targetPort: 1337, port: 1337, name: "http")
    .WithExternalHttpEndpoints()
    .WithReference(strapiDb)
    .WaitFor(strapiDb)
    .WithEnvironment("HOST", "0.0.0.0")
    .WithEnvironment("PORT", "1337")
    .WithEnvironment("NODE_ENV", "production")
    .WithEnvironment("DATABASE_CLIENT", "postgres")
    .WithEnvironment("DATABASE_HOST", postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host))
    .WithEnvironment("DATABASE_PORT", postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.TargetPort))
    .WithEnvironment("DATABASE_NAME", "strapi")
    .WithEnvironment("DATABASE_USERNAME", dbUser)
    .WithEnvironment("DATABASE_PASSWORD", dbPassword)
    .WithEnvironment("DATABASE_SSL", "false")
    .WithEnvironment("APP_KEYS", appKeys)
    .WithEnvironment("API_TOKEN_SALT", apiTokenSalt)
    .WithEnvironment("ADMIN_JWT_SECRET", adminJwtSecret)
    .WithEnvironment("TRANSFER_TOKEN_SALT", transferTokenSalt)
    .WithEnvironment("JWT_SECRET", jwtSecret)
    .WithEnvironment("ENCRYPTION_KEY", encryptionKey)
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Restart = "unless-stopped";
    });

// ---------------------------------------------------------------------------
// Blazor web frontend – consumes Strapi's GraphQL endpoint.
// ---------------------------------------------------------------------------
builder
    .AddProject<AspireStrapi_Web>("frontend-blog")
    .WithExternalHttpEndpoints()
    // Publish on host port 8090 to avoid colliding with other local services
    // (e.g. an OrbStack k8s load-balancer) that commonly occupy 8080.
    .WithEndpoint("http", endpoint => endpoint.Port = 8090)
    .WithReference(strapi.GetEndpoint("http"))
    .WaitFor(strapi)
    .WithEnvironment(
        "Strapi__GraphQlEndpoint",
        ReferenceExpression.Create($"{strapi.GetEndpoint("http")}/graphql"))
    // Browser-reachable Strapi base URL for media (cover/avatar) links. Strapi
    // is published on host port 1337 by the compose deployment, so the browser
    // loads /uploads/* from here rather than the in-network strapi:1337 host.
    .WithEnvironment("Strapi__PublicBaseUrl", "http://127.0.0.1:1337")
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Name = "frontend-blog";
        service.Restart = "unless-stopped";
    });

builder.Build().Run();
