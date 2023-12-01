using AspireStrapi.BlazorBlog.Components;

var builder = WebApplication.CreateBuilder(args);

// Add .NET Aspire services to the container.
builder.AddServiceDefaults();

// Add StrawberryShake GraphQL client services to the container.
builder.Services
    .AddBlogClient()
    .ConfigureHttpClient(client => client.BaseAddress = new Uri("http://strapi-api-dev/graphql"));

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