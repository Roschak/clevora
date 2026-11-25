namespace Clevora.Infrastructure.Data;

/// <summary>
/// Validates that the required data directory is accessible before the app accepts traffic.
/// On Linux production: /home/data. On Windows/dev: falls back gracefully.
/// </summary>
public static class MountPointValidator
{
    /// <summary>
    /// Ensures the directory containing <paramref name="dbPath"/> exists and is writable.
    /// Creates the directory if missing (dev/test environments).
    /// Throws <see cref="InvalidOperationException"/> on a production host where the
    /// directory cannot be created.
    /// </summary>
    public static void Validate(string dbPath)
    {
        if (string.IsNullOrWhiteSpace(dbPath)) return;
        var dir = Path.GetDirectoryName(Path.GetFullPath(dbPath));
        if (string.IsNullOrEmpty(dir)) return;

        if (!Directory.Exists(dir))
        {
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Data directory '{dir}' does not exist and cannot be created. " +
                    $"Ensure the volume is mounted before starting the application. Inner: {ex.Message}", ex);
            }
        }

        // Write-access probe
        var probe = Path.Combine(dir, $".probe_{Guid.NewGuid():N}");
        try
        {
            File.WriteAllText(probe, "probe");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Data directory '{dir}' is not writable. " +
                $"Check file system permissions. Inner: {ex.Message}", ex);
        }
        finally
        {
            File.Delete(probe);
        }
    }
}
