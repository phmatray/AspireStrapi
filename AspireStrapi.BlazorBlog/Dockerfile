FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AspireStrapi.BlazorBlog/AspireStrapi.BlazorBlog.csproj", "AspireStrapi.BlazorBlog/"]
RUN dotnet restore "AspireStrapi.BlazorBlog/AspireStrapi.BlazorBlog.csproj"
COPY . .
WORKDIR "/src/AspireStrapi.BlazorBlog"
RUN dotnet build "AspireStrapi.BlazorBlog.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AspireStrapi.BlazorBlog.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspireStrapi.BlazorBlog.dll"]
