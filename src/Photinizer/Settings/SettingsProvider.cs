using Photinizer.Common;
using System.Text.Json;

namespace Photinizer.Core.Settings;

internal static class SettingsProvider
{
    private static Cached<PhotinizerSettings> _settings = new(LoadFromAppsettingsOrDefault);
    public static PhotinizerSettings Get() => _settings.Get();
    
    private static readonly JsonSerializerOptions _readOptions = new() { PropertyNameCaseInsensitive = true };

    private static PhotinizerSettings LoadFromAppsettingsOrDefault()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(path))
        {
            using var data = File.OpenRead(path);
            using var doc = JsonDocument.Parse(data);

            if (doc.RootElement.TryGetProperty("Photinizer", out var section))
                return section.Deserialize<PhotinizerSettings>(_readOptions);
        }
        return new(new(), new());
    }
}