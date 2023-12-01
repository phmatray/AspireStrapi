# AspireStrapi 🚀

AspireStrapi is a demo project designed to provide a simple yet robust stack for .NET developers who are interested in using Strapi as a backend. This project utilizes a variety of technologies to create a comprehensive and efficient development environment.

## Technologies Used 💻

- .NET 8.0 Aspire
- Strapi
- Blazor App
- Postgres

## Project Setup 🛠️

Follow these steps to get started with this project:

1. **Install Strapi**: Follow the instructions on the [Strapi website](https://strapi.io/documentation/developer-docs/latest/getting-started/introduction.html) to install Strapi.

2. **Dockerize Strapi**: Use the [strapi dockerize tool](https://github.com/strapi-community/strapi-tool-dockerize) to create a production-ready Docker image of your Strapi backend.

3. **Add an Executable to the AppHost**: Add an executable that points to your backend in the AppHost. This can be done using the following code snippet:

```csharp
var strapi = builder
    .AddExecutable(
        name: "strapi-api-dev",
        command: "npm",
        workingDirectory: "../../Backend/backend-blog/",
        args: ["run", "develop"])
    .WithServiceBinding(1337, 1337, "http", "strapi-api-dev");
```

4. **Create a Blazor App**: Set up a new Blazor application as part of your project.

5. **Generate a Client for Strapi Backend**: Use StrawberryShake to generate a client for your Strapi backend. Follow the tutorial provided by StrawberryShake for guidance.

6. **Enjoy Developing!**: With the setup complete, you can now start developing your application.

## Example Usage 📖

Here's an example of how to use the `GetArticles` component in your application:

```csharp
<UseGetArticles Context="result">
    <ChildContent>
        <ul>
            @foreach (IGetArticles_Articles_Data item in result.Articles!.Data)
            {
                <li>@item.Attributes!.Title</li>
            }
        </ul>
    </ChildContent>
    <ErrorContent>
        Something went wrong ...<br />
        @result.First().Message
    </ErrorContent>
    <LoadingContent>
        Loading ...
    </LoadingContent>
</UseGetArticles>
```

This component fetches articles from the backend and displays them in a list. If an error occurs during the fetch operation, an error message is displayed. While the data is being fetched, a loading message is displayed.

## License 📄

This project is licensed under the terms of the MIT license.
