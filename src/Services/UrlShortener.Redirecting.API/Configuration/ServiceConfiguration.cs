namespace UrlShortener.Redirecting.API.Configuration;

public static class ServiceConfiguration
{
    public static IServiceCollection ConfigureDatabase(this IServiceCollection services, string mongoConnectionString)
    {
        var mongoClient = new MongoClient(mongoConnectionString);
        UrlShortenerDbContext.Create(mongoClient.GetDatabase("ShortenUrl"));

        var database = mongoClient.GetDatabase("ShortenUrl");

        var collection = database.GetCollection<ShortenedUrl>("ShortenedUrls");

        services.AddScoped(_ => collection);

        return services;
    }

    public static IServiceCollection ConfigureRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")!;

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var config = ConfigurationOptions.Parse(connectionString);
            config.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(config);
        });

        return services;
    }

    public static IServiceCollection ConfigureMediator(this IServiceCollection services)
    {
        var assembly = typeof(Program).Assembly;
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        return services;
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IAnalyticsPublisher, AnalyticsPublisher>();

        return services;
    }
}