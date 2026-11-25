using Clevora.Domain.Services;

namespace Clevora.Domain.Tests;

public class TextMetricsCalculatorTests
{
    [Theory]
    [InlineData("Hello World", 11)]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    public void CharacterCount_ReturnsCorrectLength(string? input, int expected) =>
        Assert.Equal(expected, TextMetricsCalculator.CharacterCount(input));

    [Theory]
    [InlineData("Hello World", 2)]
    [InlineData("  spaces   between   words  ", 3)]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    public void WordCount_ReturnsCorrectCount(string? input, int expected) =>
        Assert.Equal(expected, TextMetricsCalculator.WordCount(input));

    [Theory]
    [InlineData("", 0)]
    [InlineData("x", 1)]
    public void EstimatedMessageParts_SmallText_IsOne(string input, int expected) =>
        Assert.Equal(expected, TextMetricsCalculator.EstimatedMessageParts(input));

    [Fact]
    public void EstimatedMessageParts_LargeText_SplitsCorrectly()
    {
        var text = new string('a', 8193); // 2 × 4096 + 1
        Assert.Equal(3, TextMetricsCalculator.EstimatedMessageParts(text));
    }
}
