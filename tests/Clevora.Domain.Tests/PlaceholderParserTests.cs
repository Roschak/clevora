using Clevora.Domain.Services;

namespace Clevora.Domain.Tests;

public class PlaceholderParserTests
{
    [Fact]
    public void ExtractNames_ReturnsDistinctNames()
    {
        var names = PlaceholderParser.ExtractNames("Halo {{name}}, pesanan {{order}} untuk {{name}} siap.");
        Assert.Equal(2, names.Count);
        Assert.Contains("name", names);
        Assert.Contains("order", names);
    }

    [Fact]
    public void ExtractNames_EmptyString_ReturnsEmpty()
    {
        Assert.Empty(PlaceholderParser.ExtractNames(string.Empty));
    }

    [Fact]
    public void Render_ReplacesAllPlaceholders()
    {
        var values = new Dictionary<string, string> { ["name"] = "Budi", ["product"] = "Sepatu" };
        var result = PlaceholderParser.Render("Halo {{name}}, produk {{product}} siap.", values);
        Assert.Equal("Halo Budi, produk Sepatu siap.", result);
    }

    [Fact]
    public void Render_RequireAllFilled_ThrowsOnMissing()
    {
        var values = new Dictionary<string, string> { ["name"] = "Budi" };
        Assert.Throws<InvalidOperationException>(() =>
            PlaceholderParser.Render("Halo {{name}} produk {{product}}.", values, requireAllFilled: true));
    }

    [Fact]
    public void FindMissing_ReturnsOnlyUnfilledKeys()
    {
        var values = new Dictionary<string, string> { ["name"] = "Budi" };
        var missing = PlaceholderParser.FindMissing("Halo {{name}} produk {{product}}.", values);
        Assert.Single(missing);
        Assert.Equal("product", missing[0]);
    }
}
