// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

//  Original code by Matias Quaranta, see https://github.com/ealsur/GraphBulkExecutorV3

namespace CosmosGemlinBulkLoader
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public sealed class BulkOperationResponse
    {
        public TimeSpan TotalTimeTaken { get; set; }

        public int SuccessfulDocuments { get; set; } = 0;

        public double TotalRequestUnitsConsumed { get; set; } = 0;

        public IReadOnlyList<(JObject, Exception)> Failures { get; set; }
    }
}
