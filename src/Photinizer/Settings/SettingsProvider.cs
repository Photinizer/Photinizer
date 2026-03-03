using Photinizer.Common;
using System.Text.Json;

namespace Photinizer.Settings;

internal static class SettingsProvider
{
    private static readonly Cached<PhotinizerSettings> s_settings = new(LoadFromAppsettingsOrDefault);
    public static PhotinizerSettings Get() => s_settings.Get();
    
    private static readonly JsonSerializerOptions s_readOptions = new() { PropertyNameCaseInsensitive = true };

    private static PhotinizerSettings LoadFromAppsettingsOrDefault()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(path))
        {
            using var data = File.OpenRead(path);
            using var doc = JsonDocument.Parse(data);

            if (doc.RootElement.TryGetProperty("Photinizer", out var section))
                return section.Deserialize<PhotinizerSettings>(s_readOptions) ?? Fallback();
        }
        return Fallback();

        static PhotinizerSettings Fallback() => new(new WindowSettings(), new UISettings());
    }
}