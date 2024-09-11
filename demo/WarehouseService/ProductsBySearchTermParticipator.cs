﻿using Contracts;
using HandUp;

namespace WarehouseService;

public class ProductsBySearchTermParticipator : RequestParticipator<ProductsBySearchTermRequest, List<ProductBySearchTerm>>
{
    public override bool Ready(ComposeResult<List<ProductBySearchTerm>> ongoingComposeResult) => ongoingComposeResult.StructureInitialized;

    public override async Task ParticipateAsync(ProductsBySearchTermRequest request, ComposeResult<List<ProductBySearchTerm>> ongoingComposeResult)
    {
        await Task.CompletedTask;

        if (ongoingComposeResult.NotFoundOrNoResults)
        {
            return;
        }

        // todo: this would be a db lookup
        foreach (var productBySearchTerm in ongoingComposeResult.Response)
        {
            productBySearchTerm.StockLevel = productBySearchTerm.Id switch
            {
                1 => 49039,
                2 => 2934830,
                3 => 393,
                _ => throw new NotImplementedException()
            };
        }
    }
}