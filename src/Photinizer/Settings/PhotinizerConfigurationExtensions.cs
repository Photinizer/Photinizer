namespace Photinizer.Settings;

public static class PhotinizerConfigurationExtensions
{
    public static WindowConfiguration ResolveMainWindowConfiguration(this PhotinizerConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        WindowConfiguration? config = null;
        foreach ((string key, var value) in configuration.Windows)
        {
            if (key.Contains("main", StringComparison.OrdinalIgnoreCase))
            {
                config = value;
                break;
            }
            config ??= value;
        }
        return config ?? new WindowConfiguration();
    }
}
