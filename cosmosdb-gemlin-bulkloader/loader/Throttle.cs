// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

// Chris Joakim, Microsoft, May 2021

using System.Runtime.CompilerServices;

namespace CosmosGemlinBulkLoader
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Newtonsoft.Json;
    
    public class Throttle
    {
        // Instance variables:
        protected int    throttleValue;
        protected int    actualRu;
        protected int    delayMs;
        protected double multiplier = 0.0;
        protected string message;

        public Throttle(Config config, int actualRu, int overrideLevel=0)
        {
            this.throttleValue = config.GetThrottle();
            if (overrideLevel > 0)
            {
                this.throttleValue = overrideLevel;
            }
            this.multiplier = (double) Config.MAX_THROTTLE / (double) this.throttleValue;
            this.actualRu = actualRu;
            
            int baseMs = config.GetBaseThrottleTaskMilliseconds();
            int adder = (Config.MAX_THROTTLE - throttleValue) * 500;
            this.delayMs = baseMs + adder;

            Console.WriteLine(
                $"Throttle constructor; value: {throttleValue}, actualRu: {actualRu}, multiplier: {multiplier}, delay: {delayMs}");
        }

        public bool IsValid()
        {
            if (actualRu < Config.MIN_RU_SETTING)
            {
                message = $"Throttle error: Database RU setting is under {Config.MIN_RU_SETTING}: {actualRu}";
                Console.WriteLine(message);
                return false;
            }
            return true;
        }

        public string GetMessage()
        {
            return message;
        }

        public int DelayMs()
        {
            return delayMs;
        }
        
        public double Multiplier()
        {
            return multiplier;
        }
        
        /**
         * Optionally and randomly intermix no-op Delay Tasks with the actual UpsertItemAsync or
         * CreateItemAsync Tasks so as to achieve throttling so that not 100% of the Tasks will involve
         * database access.
         */
        public List<Task> AddShuffleThrottlingTasks(List<Task> tasks)
        {
            int baseCount = tasks.Count;
            double target = (double) baseCount * multiplier;
            int targetCount = (int) target;
            int additionalCount = targetCount - baseCount;
            for (int i = 0; i < additionalCount; i++)
            {
                AddTask(tasks);
            }
            List<Task> shuffledTasks = tasks.OrderBy(x => Guid.NewGuid()).ToList();
            return shuffledTasks;
        }
        
        public void AddTask(List<Task> tasks)
        {
            tasks.Add(Task.Delay(delayMs));
        }
        
        public void Display()
        {
            Console.WriteLine($"Throttle; value: {throttleValue}, ru: {actualRu}, delay: {delayMs}, multiplier: {multiplier}");
        }
        
        // Sample Display() values for the various throttle values:
        // Throttle constructor; value: 1, actualRu: 10000, multiplier: 10, delay: 16500
        // Throttle constructor; value: 2, actualRu: 10000, multiplier: 5, delay: 16000
        // Throttle constructor; value: 3, actualRu: 10000, multiplier: 3.3333333333333335, delay: 15500
        // Throttle constructor; value: 4, actualRu: 10000, multiplier: 2.5, delay: 15000
        // Throttle constructor; value: 5, actualRu: 10000, multiplier: 2, delay: 14500
        // Throttle constructor; value: 6, actualRu: 10000, multiplier: 1.6666666666666667, delay: 14000
        // Throttle constructor; value: 7, actualRu: 10000, multiplier: 1.4285714285714286, delay: 13500
        // Throttle constructor; value: 8, actualRu: 10000, multiplier: 1.25, delay: 13000
        // Throttle constructor; value: 9, actualRu: 10000, multiplier: 1.1111111111111112, delay: 12500
        // Throttle constructor; value: 10, actualRu: 10000, multiplier: 1, delay: 12000
    }
}
