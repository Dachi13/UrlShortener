var builder = WebApplication.CreateBuilder(args);

var mongoConnectionString = builder.Configuration.GetConnectionString("Default")!;

builder.Services
    .ConfigureDatabase(mongoConnectionString)
    .ConfigureRedis(builder.Configuration)
    .ConfigureServices()
    .ConfigureMediator();

builder.Services.AddScoped(_ => builder.Configuration);

var app = builder.Build();

app.AddRedirectUrlRoute();

app.Run();