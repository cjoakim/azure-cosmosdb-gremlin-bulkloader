// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

//  Original code by Matias Quaranta, see https://github.com/ealsur/GraphBulkExecutorV3

namespace CosmosGemlinBulkLoader.Element
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public sealed class GremlinVertexProperty
    {
        public object Id { get; internal set; }
        public string Key { get; internal set; }
        public object Value { get; internal set; }

        internal GremlinPropertyCollection Properties { get; set; }


        public GremlinVertexProperty(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }

        internal GremlinVertexProperty(object id, string key, object value)
        {
            this.Id = id;
            this.Key = key;
            this.Value = value;
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

        public void Validate()
        {
            if (string.IsNullOrEmpty(this.Key))
            {
                throw new Exception($"Graph Element: VertexProperty: Must have a valid Key.");
            }

            if (this.Value == null)
            {
                throw new Exception($"Graph Element: VertexProperty: Must not have a null Value.");
            }
        }

        public GremlinVertexProperty AddProperty(string key, object value)
        {
            if (this.Properties == null)
            {
                this.Properties = new GremlinPropertyCollection();
            }

            this.Properties.Add(new GremlinProperty(key, value));
            return this;
        }
    }
}
