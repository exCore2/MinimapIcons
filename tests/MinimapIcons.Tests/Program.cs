using System;
using System.Text.RegularExpressions;
using MinimapIcons.IconsBuilder;

var tests = new (string Name, Action Body)[]
{
    ("valid expression matches only configured path", ValidExpression),
    ("blank expression fails closed", BlankExpressionFailsClosed),
    ("malformed expression fails closed", MalformedExpressionFailsClosed),
    ("same expression reuses cached instance", CacheIsStable),
};

var failures = 0;
foreach (var (name, body) in tests)
{
    try
    {
        body();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        failures++;
        Console.WriteLine($"FAIL {name}: {ex.Message}");
    }
}

if (failures != 0)
    throw new InvalidOperationException($"{failures} MinimapIcons fixture test(s) failed.");

Console.WriteLine($"{tests.Length} MinimapIcons fixture tests passed.");

static void ValidExpression()
{
    var regex = RegexSafety.Get("^Metadata/Monsters/Breach/");
    Assert(regex.IsMatch("Metadata/Monsters/Breach/Host"), "matching path");
    Assert(!regex.IsMatch("Metadata/Monsters/Beast/Host"), "non-matching path");
}

static void BlankExpressionFailsClosed()
{
    var regex = RegexSafety.Get("  ");
    Assert(!regex.IsMatch("anything"), "blank expression must not match");
}

static void MalformedExpressionFailsClosed()
{
    var regex = RegexSafety.Get("[");
    Assert(!regex.IsMatch("anything"), "malformed expression must not match");
    Assert(regex.Options.HasFlag(RegexOptions.CultureInvariant), "fallback keeps invariant matching");
}

static void CacheIsStable()
{
    var first = RegexSafety.Get("^foo$");
    var second = RegexSafety.Get("^foo$");
    Assert(ReferenceEquals(first, second), "same key should reuse the compiled regex");
}

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}
