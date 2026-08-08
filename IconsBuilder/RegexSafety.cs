using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace MinimapIcons.IconsBuilder;

/// <summary>
/// Compiles user-provided icon expressions without allowing a bad setting to
/// abort the render pass. Invalid and blank expressions deliberately match
/// nothing until the setting is corrected.
/// </summary>
public static class RegexSafety
{
    private static readonly ConditionalWeakTable<string, Regex> Cache = [];

    public static Regex Get(string expression)
    {
        return Cache.GetValue(expression ?? string.Empty, static value =>
        {
            try
            {
                var pattern = string.IsNullOrWhiteSpace(value) ? "(?!)" : value;
                return new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
            }
            catch (ArgumentException)
            {
                return new Regex("(?!)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            }
        });
    }
}
