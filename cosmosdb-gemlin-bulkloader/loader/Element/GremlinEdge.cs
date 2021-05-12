// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

//  Original code by Matias Quaranta, see https://github.com/ealsur/GraphBulkExecutorV3

namespace CosmosGemlinBulkLoader.Element
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CosmosGemlinBulkLoader.Element;

    public class GremlinEdge : IGremlinElement
    {
        public string Id { get; internal set; }
        public string Label { get; internal set; }
        public object InVertexId { get; internal set; }
        public object OutVertexId { get; internal set; }
        public string InVertexLabel { get; internal set; }
        public string OutVertexLabel { get; internal set; }
        public object InVertexPartitionKey { get; internal set; }
        public object OutVertexPartitionKey { get; internal set; }

        // cj, made this variable public instead of 'internal sealed'
        // internal GremlinPropertyCollection Properties { get; set; }
        public GremlinPropertyCollection Properties { get; set; }

        public GremlinEdge(
            string edgeId,
            string edgeLabel,
            string outVertexId,
            string inVertexId,
            string outVertexLabel,
            string inVertexLabel,
            object outVertexPartitionKey = null,
            object inVertexPartitionKey = null)
        {
            this.Id = edgeId;
            this.Label = edgeLabel;
            this.InVertexId = inVertexId;
            this.InVertexLabel = inVertexLabel;
            this.InVertexPartitionKey = inVertexPartitionKey;
            this.OutVertexId = outVertexId;
            this.OutVertexLabel = outVertexLabel;
            this.OutVertexPartitionKey = outVertexPartitionKey;
        }

        internal GremlinEdge(GremlinEdge edge)
        {
            this.Id = edge.Id;
            this.Label = edge.Label;
            this.InVertexId = edge.InVertexId;
            this.OutVertexId = edge.OutVertexId;
            this.InVertexLabel = edge.InVertexLabel;
            this.OutVertexLabel = edge.OutVertexLabel;
            this.Properties = edge.Properties;
        }

        internal GremlinEdge(string id, string label)
        {
            this.Id = id;
            this.Label = label;
        }

        public GremlinProperty GetProperty(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            GremlinProperty property;
            if (!this.Properties.TryGetProperty(key, out property))
            {
                return null;
            }

            return property;
        }

        public IEnumerable<GremlinProperty> GetProperties()
        {
            return this.Properties != null ? this.Properties : Enumerable.Empty<GremlinProperty>();
        }

        public GremlinEdge AddProperty(string key, object value)
        {
            if (this.Properties == null)
            {
                this.Properties = new GremlinPropertyCollection();
            }
            this.Properties.Add(new GremlinProperty(key, value));
            return this;
        }

        public virtual void Validate()
        {
            if (this.Id == null)
            {
                throw new Exception("Graph Element: Edge: Validate: Edge must have a valid Id.");
            }

            if (string.IsNullOrEmpty(this.Label))
            {
                throw new Exception("Graph Element: Edge: Validate: Edge must have a valid Label.");
            }

            if (this.InVertexId == null)
            {
                throw new Exception("Graph Element: Edge: Validate: Edge must have a valid InVertexId.");
            }

            if (this.OutVertexId == null)
            {
                throw new Exception("Graph Element: Edge: Validate: Edge must have a valid OutVertexId.");
            }

            if (string.IsNullOrEmpty(this.InVertexLabel))
            {
                throw new Exception("Graph Element: Edge: Validate: Edge must have a valid InVertexLabel.");
            }

            if (string.IsNullOrEmpty(this.OutVertexLabel))
            {
                throw new Exception("Graph Element: Edge: Validate: Edge must specify OutVertexLabel.");
            }
        }
    }
}
