using Clevora.Domain.Services;

namespace Clevora.Domain.Tests;

public class ClickToChatUrlBuilderTests
{
    [Theory]
    [InlineData("+62 812-3456-7890", "6281234567890")]
    [InlineData("081234567890", "081234567890")]
    public void Build_StripsNonDigitsAndBuildsUrl(string phone, string digits)
    {
        var url = ClickToChatUrlBuilder.Build(phone);
        Assert.Equal($"https://wa.me/{digits}", url);
    }

    [Fact]
    public void Build_WithMessage_AppendsEncodedText()
    {
        var url = ClickToChatUrlBuilder.Build("628123", "Halo Dunia");
        Assert.Contains("text=Halo%20Dunia", url);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData(null)]
    public void Build_InvalidPhone_Throws(string? phone) =>
        Assert.Throws<ArgumentException>(() => ClickToChatUrlBuilder.Build(phone!));

    [Fact]
    public void IsValid_ValidPhone_ReturnsTrue() =>
        Assert.True(ClickToChatUrlBuilder.IsValid("+62812345"));

    [Fact]
    public void IsValid_EmptyPhone_ReturnsFalse() =>
        Assert.False(ClickToChatUrlBuilder.IsValid(string.Empty));
}
