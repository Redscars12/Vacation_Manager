using Microsoft.AspNetCore.Hosting;

namespace Vacation_Manager.Services;

public static class StoragePathHelper
{
    public static string GetUploadsRoot(IWebHostEnvironment environment)
    {
        var configuredRoot = Environment.GetEnvironmentVariable("UPLOADS_ROOT");
        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            return configuredRoot;
        }

        return Path.Combine(environment.WebRootPath, "uploads");
    }
}
