// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

//  Original code by Matias Quaranta, see https://github.com/ealsur/GraphBulkExecutorV3

namespace CosmosGemlinBulkLoader.Element
{

    public interface IGremlinElement
    {
        string Id { get; }
        string Label { get; }

        void Validate();
    }
}
