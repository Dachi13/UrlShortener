
var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();

app.AddShortenRoute();

app.Run();