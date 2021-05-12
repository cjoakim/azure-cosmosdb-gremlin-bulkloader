// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

//  Original code by Matias Quaranta, see https://github.com/ealsur/GraphBulkExecutorV3

namespace CosmosGemlinBulkLoader.Bulk
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    internal class BulkExecutor
    {
        public readonly List<Task<OperationResponse<JObject>>> Tasks;

        private readonly Stopwatch stopwatch = new Stopwatch();

        public BulkExecutor(int operationCount)
        {
            this.Tasks = new List<Task<OperationResponse<JObject>>>(operationCount);
        }

        public async Task<BulkOperationResponse> ExecuteAsync()
        {
            this.stopwatch.Start();
            await Task.WhenAll(this.Tasks);
            this.stopwatch.Stop();
            return new BulkOperationResponse()
            {
                TotalTimeTaken = this.stopwatch.Elapsed,
                TotalRequestUnitsConsumed = this.Tasks.Sum(task => task.Result.RequestUnitsConsumed),
                SuccessfulDocuments = this.Tasks.Count(task => task.Result.IsSuccessful),
                Failures = this.Tasks.Where(task => !task.Result.IsSuccessful).Select(task => (task.Result.Item, task.Result.CosmosException)).ToList()
            };
        }
    }
}
