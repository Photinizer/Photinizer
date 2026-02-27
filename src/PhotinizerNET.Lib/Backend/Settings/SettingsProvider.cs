using PhotinizerNET.Backend.Common;
using System.Text.Json;
using Options = System.Text.Json.JsonSerializerOptions;

namespace PhotinizerNET.Backend.Settings;

internal static class SettingsProvider
{
    private static Cached<PhotinizerSettings> _settings = new(LoadFromAppsettingsOrDefault);

    public static PhotinizerSettings Get() => _settings.Get();

    private static PhotinizerSettings LoadFromAppsettingsOrDefault()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(path))
        {
            using var data = File.OpenRead(path);
            using var doc = JsonDocument.Parse(data);

            if (doc.RootElement.TryGetProperty("Photinizer", out var section))
                return section.Deserialize<PhotinizerSettings>(new Options { PropertyNameCaseInsensitive = true });
        }
        return new(new(), new());
    }
}