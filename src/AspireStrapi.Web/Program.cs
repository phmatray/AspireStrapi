using AspireStrapi.Application;
using AspireStrapi.Infrastructure;
using AspireStrapi.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add .NET Aspire services to the container.
builder.AddServiceDefaults();

// Add the application use-cases (driving ports).
builder.Services.AddApplication();

// Add the Strapi GraphQL adapter and the driven-port implementations.
// The endpoint defaults to the Aspire executable name used at dev time, but can
// be overridden via configuration (e.g. the Strapi__GraphQlEndpoint env var the
// Docker Compose deployment injects so the Web container can reach the Strapi
// service by its compose service name).
var strapiGraphQlEndpoint = builder.Configuration["Strapi:GraphQlEndpoint"]
    ?? "http://strapi-api-dev/graphql";
builder.Services.AddStrapiInfrastructure(new Uri(strapiGraphQlEndpoint));

// Add RazorComponents services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map .NET Aspire endpoints.
app.MapDefaultEndpoints();

app.Run();
