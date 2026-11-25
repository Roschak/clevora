using Clevora.Infrastructure.Data;
using Xunit;

namespace Clevora.Infrastructure.Tests;

public class MountPointValidatorTests
{
    [Fact]
    public void Validate_ValidPath_CreatesDirectoryAndProbesWritable()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "clevora_test_" + Guid.NewGuid().ToString("N"));
        var dbPath = Path.Combine(tempDir, "test.db");

        try
        {
            // Act
            MountPointValidator.Validate(dbPath);

            // Assert
            Assert.True(Directory.Exists(tempDir));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyOrNullPath_DoesNotThrow(string? path)
    {
        // Act & Assert (falls back gracefully)
        MountPointValidator.Validate(path!);
    }
}
