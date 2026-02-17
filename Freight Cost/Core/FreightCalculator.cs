using System;

namespace Freight_Cost.Core;

internal static class FreightCalculator
{
    internal static decimal Calculate(decimal quote, decimal flatFee)
    {
        var multiplier = GetMultiplier(quote);
        return RoundCurrency((quote * multiplier) + flatFee);
    }

    internal static decimal GetMultiplier(decimal quote)
    {
        if (quote < 250m)
        {
            return 1.5m;
        }

        if (quote < 1000m)
        {
            return 1.33m;
        }

        return 1.2m;
    }

    internal static decimal RoundCurrency(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
