using Clevora.Domain.Services;

namespace Clevora.Domain.Tests;

public class WhatsAppFormatterTests
{
    [Theory]
    [InlineData("*bold*", "<strong>bold</strong>")]
    [InlineData("_italic_", "<em>italic</em>")]
    [InlineData("~strike~", "<s>strike</s>")]
    [InlineData("```code```", "<code>code</code>")]
    public void ToHtml_ConvertsMarkdown(string input, string expected) =>
        Assert.Equal(expected, WhatsAppFormatter.ToHtml(input));

    [Fact]
    public void ToHtml_NewlinesBecomeBr() =>
        Assert.Contains("<br/>", WhatsAppFormatter.ToHtml("line1\nline2"));

    [Fact]
    public void ToPlainText_StripsMarkdown()
    {
        var result = WhatsAppFormatter.ToPlainText("*bold* and _italic_");
        Assert.Equal("bold and italic", result);
    }

    [Fact]
    public void ToHtml_EmptyString_ReturnsEmpty() =>
        Assert.Equal(string.Empty, WhatsAppFormatter.ToHtml(string.Empty));
}
