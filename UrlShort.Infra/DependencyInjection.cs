using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using UrlShort.Application.Interfaces;
using UrlShort.Application.Services;
using UrlShort.Domain.Interfaces;
using UrlShort.Infra.Context;
using UrlShort.Infra.Repository;
using MongoDB.Driver;

namespace UrlShort.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoConnectionString = configuration.GetConnectionString("MongoDbConnection");
        var mongoDatabaseName = configuration.GetSection("MongoDbSettings:DatabaseName").Value;

        var mongoClient = new MongoClient(mongoConnectionString);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMongoDB(mongoClient, mongoDatabaseName));

        #region UrlShortService
        services.AddScoped<IUrlShortService, UrlShortService>();
        services.AddScoped<IUrlShortRepository, UrlShortRepository>();
        #endregion
        
        return services;
    }
}