// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

//  Original code by Matias Quaranta, see https://github.com/ealsur/GraphBulkExecutorV3

namespace CosmosGemlinBulkLoader.Element
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    // cj, made this class public instead of 'internal sealed'
    public class GremlinPropertyCollection : KeyedCollection<string, GremlinProperty>
    {
        internal GremlinPropertyCollection()
        {
        }

        internal GremlinPropertyCollection(IEnumerable<GremlinProperty> items)
        {
            if (items != null)
            {
                this.AddRange(items);
            }
        }

        public bool TryGetProperty(string key, out GremlinProperty property)
        {
            if (this.Dictionary != null)
            {
                return this.Dictionary.TryGetValue(key, out property);
            }

            property = null;
            return false;
        }

        internal void AddRange(IEnumerable<GremlinProperty> items)
        {
            if (items == null)
            {
                throw new Exception($"Graph Element: Property Collection: Add Range: null {nameof(items)}");
            }

            foreach (GremlinProperty property in items)
            {
                this.Add(property);
            }
        }

        protected override string GetKeyForItem(GremlinProperty item)
        {
            return item.Key;
        }
    }
}
