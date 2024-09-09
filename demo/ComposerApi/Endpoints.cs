using Contracts;
using HandUp;
using Microsoft.AspNetCore.Mvc;

namespace ComposerApi;

public static class Endpoints
{
    public static async Task<IResult> GetProductById([FromServices] IComposeServices serviceComposer, int productId)
    {
        var request = new ProductByIdRequest(productId);
        
        var response = new ProductByIdResponse();

        await serviceComposer.ComposeAsync(request, response);

        return Results.Ok(response);
    }
}