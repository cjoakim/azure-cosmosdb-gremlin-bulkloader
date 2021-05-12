// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

//  Original code by Matias Quaranta, see https://github.com/ealsur/GraphBulkExecutorV3

namespace CosmosGemlinBulkLoader.Bulk
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;

    internal static class TaskExtensions
    {
        public static Task<OperationResponse<T>> CaptureOperationResponse<T>(this Task<ItemResponse<T>> task, T item)
        {
            return task.ContinueWith(itemResponse =>
            {
                if (itemResponse.Status == TaskStatus.RanToCompletion 
                    && !itemResponse.IsFaulted 
                    && !itemResponse.IsCanceled)
                {
                    return new OperationResponse<T>()
                    {
                        Item = item,
                        IsSuccessful = true,
                        RequestUnitsConsumed = task.Result.RequestCharge
                    };
                }

                AggregateException innerExceptions = itemResponse.Exception.Flatten();
                if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                {
                    return new OperationResponse<T>()
                    {
                        Item = item,
                        RequestUnitsConsumed = cosmosException.RequestCharge,
                        IsSuccessful = false,
                        CosmosException = cosmosException
                    };
                }

                return new OperationResponse<T>()
                {
                    Item = item,
                    IsSuccessful = false,
                    CosmosException = innerExceptions.InnerExceptions.FirstOrDefault()
                };
            });
        }
    }
}
