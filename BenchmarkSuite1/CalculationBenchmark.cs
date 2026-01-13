using BenchmarkDotNet.Attributes;
using System;
using System.Globalization;
using Microsoft.VSDiagnostics;

namespace Freight_Cost.Benchmarks
{
    [CPUUsageDiagnoser]
    public class CalculationBenchmark
    {
        private decimal[] quotes;
        [GlobalSetup]
        public void Setup()
        {
            quotes = new decimal[]
            {
                10m,
                249.99m,
                250m,
                999.99m,
                1000m,
                1500m
            };
        }

        [Benchmark]
        public decimal CalculateMany()
        {
            decimal total = 0m;
            for (int i = 0; i < quotes.Length; i++)
            {
                var q = quotes[i];
                // call into the application's calculation helpers
                var mul = Freight_Cost.Form1.GetMultiplier(q);
                var freight = Freight_Cost.Form1.RoundCents((q * mul) + 0m);
                total += freight;
            }

            return total;
        }
    }
}