// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

using System;
using Xunit;
using CosmosGemlinBulkLoader;
using CosmosGemlinBulkLoader.Csv;

namespace loader.tests
{
    public class ThrottleTest
    {
        [Fact]
        public void InsufficientRuTest()
        {
            string[] args = "load --file-type edge --csv-infile".Split();
            Config c = new Config(args);
            string expected = "Throttle error: Database RU setting is under 5000: 400";
            Throttle t = new Throttle(c, 400);
            Assert.False(t.IsValid()); 
            Assert.True(t.GetMessage() == expected);
        }
        
        [Fact]
        public void ThrottleLevelsTest()
        {
            string[] args = "load --throttle 10 --file-type edge --csv-infile".Split();
            Config c = new Config(args);
            Throttle t = new Throttle(c, 10000);
            Assert.True(t.IsValid());
            Assert.True(t.DelayMs() == Config.BASE_THROTTLE_TASK_MS);
            Assert.True(t.DelayMs() == 12000);
            Assert.True(t.Multiplier() == 1.0);
            
            args = "load --throttle 9 --file-type edge --csv-infile".Split();
            c = new Config(args);
            t = new Throttle(c, 10000);
            Assert.True(t.IsValid());
            Assert.True(t.DelayMs() == 12500);
            double expected = Config.MAX_THROTTLE / 9.0;
            Assert.True(t.Multiplier() == expected);
            
            args = "load --throttle 2 --file-type edge --csv-infile".Split();
            c = new Config(args);
            t = new Throttle(c, 10000);
            Assert.True(t.IsValid());
            Assert.True(t.DelayMs() == 16000);
            expected = Config.MAX_THROTTLE / 2.0;
            Assert.True(t.Multiplier() == expected);
            
            args = "load --throttle 1 --file-type edge --csv-infile".Split();
            c = new Config(args);
            t = new Throttle(c, 10000);
            Assert.True(t.IsValid());
            Assert.True(t.DelayMs() == 16500);
            expected = Config.MAX_THROTTLE / 1.0;
            Assert.True(t.Multiplier() == expected);
        }
   }
}
