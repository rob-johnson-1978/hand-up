using ComposerApi;
using CustomerService;
using HandUp;
using HandUp.AspNet;
using ProductDetailsService;
using ProductPricingService;
using ProductReviewService;
using WarehouseService;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    .AddHandUp(
        configuration =>
        {
            configuration.MaxParticipationLoopCount = 20;
            
            configuration
                .AddConfigurator(new ProductDetailsServiceHandUpConfigurator())
                .AddConfigurator(new ProductPricingServiceHandUpConfigurator())
                .AddConfigurator(new ProductReviewServiceHandUpConfigurator())
                .AddConfigurator(new WarehouseServiceHandUpConfigurator())
                .AddConfigurator(new CustomerServiceHandUpConfigurator());
        })
    .AddHandUpAspNet();

var app = builder.Build();

app.MapGet("/livez", () => Results.Ok("...and it's LIVE!"));

app.MapGet("/product/{productId:int}", Endpoints.GetProductById);
app.MapGet("/products", Endpoints.GetProductsBySearchTerm);
app.MapGet("/async-product/{productId:int}", Endpoints.GetProductByIdForAsync);
app.MapGet("/async-products", Endpoints.GetProductsBySearchTermForAsync);
app.MapPost("/checkout/complete", Endpoints.CompleteCheckout);

app.Run();
