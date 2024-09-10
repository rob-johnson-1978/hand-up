using Contracts;
using HandUp.AspNet;
using Microsoft.AspNetCore.Mvc;

namespace ComposerApi;

public static class Endpoints
{
    public static async Task<IResult> GetProductById([FromServices] IComposeServicesForMinimalApis serviceComposer, [FromRoute] int productId) =>
        await serviceComposer.ComposeEndpointAsync(
            request: new ProductByIdRequest(productId),
            response: new ProductByIdResponse()
        );

    public static async Task<IResult> GetProductsBySearchTerm([FromServices] IComposeServicesForMinimalApis serviceComposer, [FromQuery] string searchTerm) =>
        await serviceComposer.ComposeEndpointAsync(
            request: new ProductsBySearchTermRequest(searchTerm),
            response: new List<ProductBySearchTerm>(),
            onNotFoundOrNoResults: Results.Ok
        );

    public static async Task<IResult> GetProductByIdForAsync([FromServices] IComposeServicesForMinimalApis serviceComposer, [FromRoute] int productId) =>
        await serviceComposer.ComposeEndpointAsync(
            requestForAsync: new ProductByIdRequest(productId),
            responseForAsync: new ProductByIdResponse()
        );

    public static async Task<IResult> GetProductsBySearchTermForAsync([FromServices] IComposeServicesForMinimalApis serviceComposer, [FromQuery] string searchTerm) =>
        await serviceComposer.ComposeEndpointAsync(
            requestForAsync: new ProductsBySearchTermRequest(searchTerm),
            responseForAsync: new List<ProductBySearchTerm>(),
            onSuccessAsync: async response =>
            {
                await Task.CompletedTask;
                return Results.Ok(response);
            },
            onNotFoundOrNoResultsAsync: async response =>
            {
                await Task.CompletedTask;
                return Results.Ok(response);
            }, 
            onErrorAsync: async errors =>
            {
                await Task.CompletedTask;
                return Results.Problem(string.Join(" / ", errors));
            }
        );
}