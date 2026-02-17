using System.Globalization;
using System.Text;

namespace Freight_Cost.Core;

internal static class CurrencyInput
{
    internal static readonly CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");

    internal static bool TryParseUsd(string? text, out decimal value, out string error)
    {
        error = string.Empty;
        value = 0m;

        var trimmed = text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            error = "Enter an amount (example: 12.34 or $12.34).";
            return false;
        }

        if (!decimal.TryParse(trimmed, NumberStyles.Currency, UsCulture, out value))
        {
            error = "Enter a valid USD amount. Examples: 12.34, $12.34, $1,234.56.";
            return false;
        }

        if (value < 0m)
        {
            error = "Amount cannot be negative.";
            return false;
        }

        return true;
    }

    internal static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var input = raw.Trim();
        var normalized = new StringBuilder(input.Length);
        var hasDot = false;
        var hasDollar = false;

        foreach (var ch in input)
        {
            if (char.IsDigit(ch))
            {
                normalized.Append(ch);
                continue;
            }

            if (ch == '.' && !hasDot)
            {
                normalized.Append(ch);
                hasDot = true;
                continue;
            }

            if (ch == ',')
            {
                normalized.Append(ch);
                continue;
            }

            if (ch == '$' && !hasDollar && normalized.Length == 0)
            {
                normalized.Append(ch);
                hasDollar = true;
            }
        }

        return normalized.ToString();
    }
}
