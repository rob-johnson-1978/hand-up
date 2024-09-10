using Contracts;
using HandUp;
using Microsoft.AspNetCore.Mvc;

namespace ComposerApi;

public static class Endpoints
{
    public static async Task<IResult> GetProductById([FromServices] IComposeServices serviceComposer, [FromRoute] int productId)
    {
        var request = new ProductByIdRequest(productId);
        
        var response = new ProductByIdResponse();

        await serviceComposer.ComposeAsync(request, response);

        return Results.Ok(response);
    }

    public static async Task<IResult> GetProductsBySearchTerm([FromServices] IComposeServices serviceComposer, [FromQuery] string searchTerm)
    {
        var request = new ProductsBySearchTermRequest(searchTerm);

        var response = new List<ProductBySearchTerm>();

        await serviceComposer.ComposeAsync(request, response);

        return Results.Ok(response);
    }
}