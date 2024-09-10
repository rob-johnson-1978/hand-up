using ComposerApi;
using HandUp;
using ProductDetailsService;
using ProductPricingService;
using ProductReviewService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHandUp(
    configuration =>
    {
        configuration.MaxParticipationLoopCount = 20;
        
        configuration
            .AddConfigurator(new ProductDetailsServiceHandUpConfigurator())
            .AddConfigurator(new ProductPricingServiceHandUpConfigurator())
            .AddConfigurator(new ProductReviewServiceHandUpConfigurator());
    });

var app = builder.Build();

app.MapGet("/livez", () => Results.Ok("...and it's LIVE!"));

app.MapGet("/product/{productId:int}", Endpoints.GetProductById);
app.MapGet("/products", Endpoints.GetProductsBySearchTerm);

app.Run();
