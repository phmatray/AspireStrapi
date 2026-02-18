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
    .WithHttpEndpoint(port: 1337, targetPort: 1337, name: "http");

builder
    .AddProject<AspireStrapi_BlazorBlog>("frontend-blog")
    .WithReference(strapi.GetEndpoint("http"));

builder.Build().Run();

// class StrapiExecutable : ExecutableResource
// {
//     public StrapiExecutable(string name, string command, string workingDirectory, string[]? args)
//         : base(name, command, workingDirectory, args)
//     {
//     }
//     
// }
