// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

//  Original code by Matias Quaranta, see https://github.com/ealsur/GraphBulkExecutorV3

namespace CosmosGemlinBulkLoader.Element
{
    using System;

    public sealed class GremlinProperty : IEquatable<GremlinProperty>
    {
        public string Key { get; internal set; }
        public object Value { get; internal set; }

        internal GremlinProperty()
        {
        }

        internal GremlinProperty(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }

        public bool Equals(GremlinProperty other)
        {
            if (other == null)
            {
                return false;
            }

            if (string.Equals(this.Key, other.Key, StringComparison.OrdinalIgnoreCase) &&
                this.Value.GetType() == other.Value.GetType())
            {
                return this.Value.Equals(other.Value);
            }

            return false;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(this.Key))
            {
                throw new Exception($"Graph Element: GremlinProperty: Validate: {nameof(this.Key)} must have a valid {nameof(this.Key)}.");
            }

            if (this.Value == null)
            {
                throw new Exception($"Graph Element: GremlinProperty: Validate: {nameof(this.Value)} must have a valid {nameof(this.Value)}.");
            }
        }
    }
}
