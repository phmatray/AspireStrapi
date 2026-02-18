# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AspireStrapi is a full-stack blog application demonstrating .NET Aspire integration with Strapi CMS:
- **Frontend**: Blazor web application with GraphQL client (StrawberryShake)
- **Backend**: Strapi headless CMS (Node.js)
- **Orchestration**: .NET Aspire AppHost manages all services
- **API**: GraphQL communication between frontend and backend

## Essential Commands

### Starting Development
```bash
# Run everything through Aspire (recommended)
dotnet run --project AspireStrapi/AspireStrapi.AppHost/AspireStrapi.AppHost.csproj
```
This starts both Strapi (port 1337) and Blazor frontend with proper service references.

### Building the Solution
```bash
# Build entire solution
dotnet build AspireStrapi.sln

# Build for production
dotnet build AspireStrapi.sln --configuration Release
```

### Working with Strapi
```bash
cd Backend/backend-blog
npm install          # First time setup
npm run develop      # Development mode
npm run build        # Production build
```

### Updating GraphQL Client
When Strapi schema changes:
```bash
cd AspireStrapi.BlazorBlog
dotnet graphql generate
```

## Architecture

### Service Structure
- **AspireStrapi.AppHost**: Orchestrates all services, configures Strapi as executable
- **AspireStrapi.BlazorBlog**: Frontend with GraphQL queries in `ApiClient/` folder
- **AspireStrapi.ServiceDefaults**: Shared configurations
- **Backend/backend-blog**: Strapi CMS with content types for articles, authors, categories

### Key Integration Points
1. AppHost starts Strapi using `AddExecutable()` on port 1337
2. Blazor app references Strapi through Aspire service discovery
3. GraphQL client generated from `ApiClient/schema.graphql`
4. StrawberryShake components provide typed access to Strapi data

### Content Types in Strapi
- `article`: Blog posts with title, content, author, category
- `author`: Article authors
- `category`: Article categories
- `about`: About page content
- `global`: Site-wide settings

## Development Notes

- Node.js version must be 18.0.0 - 20.x.x for Strapi
- .NET 9.0 required
- GraphQL endpoint: http://localhost:1337/graphql
- Strapi admin panel: http://localhost:1337/admin
- Sample data included in `Backend/backend-blog/data/`