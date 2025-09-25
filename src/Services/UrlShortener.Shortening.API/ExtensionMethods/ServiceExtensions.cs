namespace UrlShortener.Shortening.API.ExtensionMethods;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureDatabase(this IServiceCollection services, string mongoConnectionString)
    {
        var mongoClient = new MongoClient(mongoConnectionString);
        UrlShortenerDbContext.Create(mongoClient.GetDatabase("ShortenUrl"));

        var database = mongoClient.GetDatabase("ShortenUrl");
        var collections = database.ListCollectionNames().ToList();

        var collection = database.GetCollection<ShortenedUrl>("ShortenedUrls");

        services.AddScoped(_ => collection);

        if (!collections.Contains("ShortenedUrls"))
        {
            Console.WriteLine("Creating collection 'ShortenedUrls'...");
            database.CreateCollection("ShortenedUrls");
        }
        else Console.WriteLine("Collection 'ShortenedUrls' already exists.");

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
}