using AspireStrapi.Application.Ports;
using AspireStrapi.Domain.Entities;
using AspireStrapi.Domain.ValueObjects;
using AspireStrapi.Infrastructure.ApiClient;
using StrawberryShake;

namespace AspireStrapi.Infrastructure.Adapters;

/// <summary>
/// Outbound adapter that fetches categories (with their article counts) from the
/// Strapi GraphQL API and maps them into domain <see cref="Category"/> entities.
/// </summary>
public sealed class StrapiCategoryRepository : ICategoryRepository
{
    private readonly IBlogClient _client;

    public StrapiCategoryRepository(IBlogClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        IOperationResult<IGetCategoriesResult> result =
            await _client.GetCategories.ExecuteAsync(cancellationToken);

        result.EnsureNoErrors();

        IReadOnlyList<IGetCategories_Categories?>? categories = result.Data?.Categories;
        if (categories is null)
        {
            return [];
        }

        return categories
            .Where(item => item?.Name is not null)
            .Select(item => new Category(
                name: item!.Name!,
                slug: string.IsNullOrWhiteSpace(item.Slug) ? null : new Slug(item.Slug),
                description: item.Description,
                id: item.DocumentId,
                articleCount: item.Articles_connection?.Nodes.Count ?? 0))
            .ToList();
    }
}
