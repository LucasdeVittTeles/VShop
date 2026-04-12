using VShop.ProductApi.Repositories;
using VShop.ProductApi.Repositories.Interfaces;
using VShop.ProductApi.Services.Interfaces;
using VShop.ProductApi.Services;

namespace VShop.ProductApi;

public static class DomainExtensions
{

    public static IServiceCollection AddCoreServicesDependencies(this IServiceCollection services)
    {

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }

    public static IServiceCollection AddCoreRepositoryDependencies(this IServiceCollection services)
    {
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }


}
