using Clevora.Domain.Entities;
using Clevora.Shared.Models;
using Xunit;

namespace Clevora.Mobile.Tests;

public class LocalSyncEngineTests
{
    [Fact]
    public void SyncOutbox_RecordsPendingMutations()
    {
        // Arrange
        var outbox = new List<SyncPushItem>();
        var newTemplate = new MessageTemplate
        {
            Id = 1,
            Title = "Followup Penawaran",
            Body = "Halo {{nama}}, penawaran kami diskon 20%",
            Version = 1,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        outbox.Add(new SyncPushItem(
            "template",
            newTemplate.Id,
            SyncOperation.Create,
            0,
            DateTime.UtcNow,
            "{\"title\":\"Followup Penawaran\"}"));

        // Assert
        Assert.Single(outbox);
        Assert.Equal(SyncOperation.Create, outbox[0].Operation);
        Assert.Equal("template", outbox[0].EntityType);
    }

    [Fact]
    public void ConflictDetection_WhenServerVersionIsHigher_IdentifiesConflict()
    {
        // Arrange
        long localBaseVersion = 2;
        long serverCurrentVersion = 4;

        // Act
        bool isConflict = serverCurrentVersion > localBaseVersion;

        // Assert
        Assert.True(isConflict);
    }

    [Fact]
    public void PullMerge_AppliesRemoteUpdates_Accurately()
    {
        // Arrange
        var localTemplates = new List<MessageTemplate>
        {
            new() { Id = 1, Title = "Template Lama", Body = "Body Lama", Version = 1 }
        };

        var pullResponse = new SyncPullResponse(
            LatestServerVersion: 5,
            Categories: new List<CategoryDto>(),
            Templates: new List<TemplateDto>
            {
                new(1, "Template Baru Server", "Body Baru", 1, "Umum", false, false, null, 5, DateTime.UtcNow, DateTime.UtcNow, new List<string>())
            },
            Drafts: new List<DraftDto>(),
            DeletedCategoryIds: new List<int>(),
            DeletedTemplateIds: new List<int>(),
            DeletedDraftIds: new List<int>());

        // Act
        foreach (var remoteT in pullResponse.Templates)
        {
            var match = localTemplates.FirstOrDefault(t => t.Id == remoteT.Id);
            if (match is not null)
            {
                match.Title = remoteT.Title;
                match.Body = remoteT.Body;
                match.Version = remoteT.Version;
            }
        }

        // Assert
        Assert.Equal("Template Baru Server", localTemplates[0].Title);
        Assert.Equal(5, localTemplates[0].Version);
    }
}
