using ComposerApi;
using HandUp;
using ProductDetailsService;
using ProductPricingService;
using ProductReviewService;
using SalesService;
using WarehouseService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHandUp(
    configuration =>
    {
        configuration.MaxParticipationLoopCount = 20;
        
        configuration
            .AddConfigurator(new ProductDetailsServiceHandUpConfigurator())
            .AddConfigurator(new ProductPricingServiceHandUpConfigurator())
            .AddConfigurator(new ProductReviewServiceHandUpConfigurator())
            .AddConfigurator(new WarehouseServiceHandUpConfigurator())
            .AddConfigurator(new SalesServiceHandUpConfigurator());
    });

var app = builder.Build();

app.MapGet("/livez", () => Results.Ok("...and it's LIVE!"));

app.MapGet("/product/{productId:int}", Endpoints.GetProductById);
app.MapGet("/products", Endpoints.GetProductsBySearchTerm);

app.Run();
