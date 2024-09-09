var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/livez", () => Results.Ok("...and it's LIVE!"));

app.Run();
