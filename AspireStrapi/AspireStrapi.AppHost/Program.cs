using Microsoft.Extensions.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// builder.AddPostgresContainer("db")
//     .AddDatabase("strapi");

var strapi = builder
    .AddExecutable(
        name: "strapi-api-dev",
        command: "npm",
        workingDirectory: "../../Backend/backend-blog/",
        args: ["run", "develop"])
    .WithServiceBinding(1337, 1337, "http", "strapi-api-dev");

builder
    .AddProject<AspireStrapi_BlazorBlog>("frontend-blog")
    .WithReference(strapi.GetEndpoint("strapi-api-dev"));

builder.Build().Run();

// class StrapiExecutable : ExecutableResource
// {
//     public StrapiExecutable(string name, string command, string workingDirectory, string[]? args)
//         : base(name, command, workingDirectory, args)
//     {
//     }
//     
// }
