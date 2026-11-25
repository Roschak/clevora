using Clevora.Domain.Services;
using Clevora.Shared.Models;
using Xunit;

namespace Clevora.Api.Tests;

public class ComposerEndpointTests
{
    [Fact]
    public void ComposerPreview_GeneratesHtmlAndUrl_Successfully()
    {
        // Arrange
        var body = "Halo *{{nama}}*, pesanan {{produk}} sudah dikirim!";
        var values = new Dictionary<string, string>
        {
            { "nama", "Budi" },
            { "produk", "Sepatu Kulit" }
        };
        var phone = "08123456789";

        // Act
        var missing = PlaceholderParser.FindMissing(body, values);
        var rendered = PlaceholderParser.Render(body, values);
        var html = WhatsAppFormatter.ToHtml(rendered);
        var charCount = TextMetricsCalculator.CharacterCount(rendered);
        var wordCount = TextMetricsCalculator.WordCount(rendered);
        var url = ClickToChatUrlBuilder.Build(phone, rendered);

        // Assert
        Assert.Empty(missing);
        Assert.Contains("<strong>Budi</strong>", html);
        Assert.True(charCount > 0);
        Assert.True(wordCount > 0);
        Assert.StartsWith("https://wa.me/08123456789?text=", url);
    }

    [Fact]
    public void ComposerPreview_DetectsMissingPlaceholders()
    {
        // Arrange
        var body = "Halo {{nama}}, kode voucher Anda: {{voucher}}";
        var values = new Dictionary<string, string>
        {
            { "nama", "Siti" }
        };

        // Act
        var missing = PlaceholderParser.FindMissing(body, values);

        // Assert
        Assert.Single(missing);
        Assert.Equal("voucher", missing[0]);
    }

    [Fact]
    public void SyncContract_SerializesAndDeserializes_Correctly()
    {
        // Arrange
        var item = new SyncPushItem("template", 101, SyncOperation.Update, 3, DateTime.UtcNow, "{\"title\":\"Test\"}");
        var request = new SyncPushRequest("device-xyz", new List<SyncPushItem> { item });

        // Assert
        Assert.Equal("device-xyz", request.DeviceId);
        Assert.Single(request.Items);
        Assert.Equal(SyncOperation.Update, request.Items[0].Operation);
        Assert.Equal(3, request.Items[0].BaseVersion);
    }
}
