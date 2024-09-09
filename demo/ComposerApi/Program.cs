using ComposerApi;
using HandUp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHandUp();

var app = builder.Build();

app.MapGet("/livez", () => Results.Ok("...and it's LIVE!"));

app.MapGet("/product/{productId:int}", Endpoints.GetProductById);

app.Run();
