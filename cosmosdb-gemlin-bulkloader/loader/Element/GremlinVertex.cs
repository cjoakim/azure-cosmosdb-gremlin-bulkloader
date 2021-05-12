// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

//  Original code by Matias Quaranta, see https://github.com/ealsur/GraphBulkExecutorV3

namespace CosmosGemlinBulkLoader.Element
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    public class GremlinVertex : IGremlinElement
    {
        public string Id { get; internal set; }
        public string Label { get; internal set; }

        // cj, made this variable public instead of 'internal sealed'
        // internal Dictionary<string, List<GremlinVertexProperty>> Properties { get; set; }
        public Dictionary<string, List<GremlinVertexProperty>> Properties { get; set; }


        public GremlinVertex(string id, string label)
        {
            this.Id = id;
            this.Label = label;
        }
        
        public void AddProperty(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception($"GremlinVertex, AddProperty: Must have a valid Key.");
            }

            if (value == null)
            {
                throw new Exception($"GremlinVertex, AddProperty: Must not have a null Value.");
            }

            if (this.Properties == null)
            {
                this.Properties = new Dictionary<string, List<GremlinVertexProperty>>();
            }

            List<GremlinVertexProperty> propertiesForKey = null;

            if (!this.Properties.TryGetValue(key, out propertiesForKey))
            {
                propertiesForKey = new List<GremlinVertexProperty>();
                this.Properties.Add(key, propertiesForKey);
            }

            propertiesForKey.Add(new GremlinVertexProperty(key, value));
        }

        public GremlinVertexProperty AddProperty(GremlinVertexProperty vertexProperty)
        {
            vertexProperty.Validate();

            if (this.Properties == null)
            {
                this.Properties = new Dictionary<string, List<GremlinVertexProperty>>();
            }

            List<GremlinVertexProperty> propertiesForKey = null;
            if (!this.Properties.TryGetValue(vertexProperty.Key, out propertiesForKey))
            {
                propertiesForKey = new List<GremlinVertexProperty>();
                this.Properties.Add(vertexProperty.Key, propertiesForKey);
            }

            propertiesForKey.Add(vertexProperty);
            return vertexProperty;
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return this.Properties != null ? this.Properties.Keys : Enumerable.Empty<string>();
        }

        public IEnumerable<GremlinVertexProperty> GetVertexProperties(string key)
        {
            if (key == null)
            {
                throw new Exception($"GremlinVertex, GetVertexProperties: Get Property {nameof(key)}");
            }

            return this.Properties != null ? this.GetNestedEnumerable(this.Properties, key) : Enumerable.Empty<GremlinVertexProperty>();
        }

        public IEnumerable<GremlinVertexProperty> GetVertexProperties()
        {
            return this.Properties != null ? this.GetNestedEnumerable(this.Properties) : Enumerable.Empty<GremlinVertexProperty>();
        }

        public void Validate()
        {
            if (this.Id == null)
            {
                throw new Exception($"Graph Element: Vertex: Validate: Vertex must have a valid Id.");
            }

            if (string.IsNullOrEmpty(this.Label))
            {
                throw new Exception($"Graph Element: Vertex: Validate: Vertex must have a valid Label.");
            }
        }

        internal IEnumerable<T> GetNestedEnumerable<T>(Dictionary<string, List<T>> dictionary)
        {
            return dictionary.Keys.SelectMany(k => this.GetNestedEnumerable<T>(dictionary, k));
        }

        internal IEnumerable<T> GetNestedEnumerable<T>(Dictionary<string, List<T>> dictionary, string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            List<T> valueList = dictionary[key];
            foreach (T value in valueList)
            {
                yield return value;
            }
        }
    }
}
