using ComposerApi;
using HandUp;
using ProductDetailsService;
using ProductPricingService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHandUp(
    opts =>
    {
        opts
            .AddConfigurator(new ProductDetailsServiceHandUpConfigurator())
            .AddConfigurator(new ProductPricingServiceHandUpConfigurator());
    });

var app = builder.Build();

app.MapGet("/livez", () => Results.Ok("...and it's LIVE!"));

app.MapGet("/product/{productId:int}", Endpoints.GetProductById);

app.Run();
